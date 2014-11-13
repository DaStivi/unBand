﻿using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unBand.BandHelpers.Sensors;

namespace unBand.BandHelpers
{
    class BandSensors
    {

        private CargoClient _client;

        public BandPedometer Pedometer { get; set; }

        public BandSensors(CargoClient client)
        {
            _client = client;

            Init();
        }

        private void Init()
        {
            Pedometer = new BandPedometer(_client);
        }

    }
}
