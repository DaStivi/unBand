﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Microsoft.Win32;
using Mono.Cecil;

namespace unBand.Cloud
{
    public static class CargoDll
    {
        private static readonly List<string> _bandDlls = new List<string>
        {
            "Microsoft.Band.Admin.Desktop",
            "Microsoft.Band.Admin",
            "Microsoft.Band.Desktop",
            "Microsoft.Band"
        };

        public static bool BandDllsExist(ref string message)
        {
            try
            {
                GetOfficialBandDllPath();
            }
            catch (Exception e)
            {
                message = e.Message;

                return false;
            }

            return true;
        }

        public static string GetUnBandBandDll(string dllName, string outputPath = null)
        {
            if (outputPath == null)
            {
                outputPath = GetUnBandAppDataDir();
            }

            var officialDllPath = GetOfficialBandDllPath();

            var officialDll = Path.Combine(officialDllPath, dllName + ".dll");
            var unbandDll = Path.Combine(outputPath, dllName + ".dll");

            if (!(File.Exists(unbandDll) && GetVersion(officialDll) == GetVersion(unbandDll)))
            {
                CreateUnBandCargoDll(officialDll, unbandDll);
            }

            return unbandDll;
        }

        public static string GenerateUnbandDlls(string unbandBandDllPath)
        {
            if (unbandBandDllPath == null)
            {
                unbandBandDllPath = GetUnBandAppDataDir();
            }

            foreach (var dllName in _bandDlls)
            {
                GetUnBandBandDll(dllName, unbandBandDllPath);
            }

            return unbandBandDllPath;
        }

        private static void CreateUnBandCargoDll(string officialDll, string unBandCargoDll)
        {
            var module = ModuleDefinition.ReadModule(officialDll);

            // make everything public - this little bit of magic (that hopefully no one will ever see, because
            // it's horrible) will allow us to extend the dll
            foreach (var type in module.Types)
            {
                type.IsPublic = true;

                // we also need to make CargoClient's constructor public (so that we can avoid various checks)

                if (type.Name == "CargoClient")
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.Name.Contains(".ctor"))
                        {
                            method.IsPublic = true;
                        }
                    }

                    // modify internal fields. If we were in a minefield before imagine where we are now?
                    // (these have no properties, or no properties with a setter)
                    foreach (var field in type.Fields)
                    {
                        if (field.Name.Contains("deviceTransportApp"))
                        {
                            field.IsPublic = true;
                        }
                    }
                }
                else if (type.Name == "CargoStreamReader" || type.Name == "CargoStreamWriter" ||
                         type.Name == "BufferServer")
                {
                    foreach (var method in type.Methods)
                    {
                        method.IsPublic = true;
                    }
                }
            }

            module.Write(unBandCargoDll);
        }

        private static string GetVersion(string dllPath)
        {
            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(dllPath);

                return assembly.Name.Version.ToString();
            }
            catch
            {
                return "Invalid DLL";
            }
        }

        private static string GetUnBandAppDataDir()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "unBand");

            // TODO: creating a file here feels like a dirty side affect
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir);
        }

        public static string GetOfficialBandDllPath()
        {
            // let's try and find the dll
            var dllLocations = new List<string>
            {
                GetDllLocationFromRegistry(),

                // fallback path
                Path.Combine(GetProgramFilesx86(), "Microsoft Band Sync")
            };

            var errors = "";

            foreach (var path in dllLocations)
            {
                if (IsValidBandDesktopAppPath(path, ref errors))
                    return path;
            }

            throw new FileNotFoundException("Could not find Band Sync app on your machine. I looked in:\n\n" +
                                            string.Join("\n", dllLocations) + "\n\nExtra errors:\n" + errors);
        }

        private static bool IsValidBandDesktopAppPath(string path, ref string message)
        {
            if (!Directory.Exists(path))
                return false;

            foreach (var dll in _bandDlls)
            {
                var file = Path.Combine(path, dll + ".dll");

                if (!File.Exists(file))
                {
                    message += "Could not find file: " + file + "\n";
                    return false;
                }
            }

            return true;
        }

        private static string GetDllLocationFromRegistry()
        {
            var sid = WindowsIdentity.GetCurrent().User.ToString();

            var regRoot = RegistryHive.LocalMachine;
            var regKeyName = @"SOFTWARE\MICROSOFT\Windows\CurrentVersion\Installer\UserData\" + sid +
                             @"\Components\23439AC101C46D55BBCE6A082085E137";
            var regValueName = "6A5C0F782DABC24499D24EB7E14D7951";

            RegistryKey regKey;

            if (Environment.Is64BitOperatingSystem)
            {
                regKey = RegistryKey.OpenBaseKey(regRoot, RegistryView.Registry64);
            }
            else
            {
                regKey = RegistryKey.OpenBaseKey(regRoot, RegistryView.Default);
            }

            regKey = regKey.OpenSubKey(regKeyName);

            if (regKey != null)
            {
                var value = regKey.GetValue(regValueName);

                if (value != null)
                {
                    var path = Path.GetDirectoryName(value.ToString());

                    if (!Directory.Exists(path))
                    {
                        return "[key found, but dir invalid] " + path;
                    }

                    return path;
                }
            }

            return "[not found] " + regKeyName + "\\" + regValueName;
        }

        private static string GetProgramFilesx86()
        {
            var envVar = (Environment.Is64BitProcess ? "ProgramFiles(x86)" : "ProgramFiles");

            return Environment.GetEnvironmentVariable(envVar);
        }
    }
}