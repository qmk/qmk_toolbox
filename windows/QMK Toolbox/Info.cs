//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using Newtonsoft.Json;

namespace QMK_Toolbox
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class Info
    {
        [JsonProperty]
        public string Keyboard { get; set; }

        [JsonProperty]
        public string Keymap { get; set; }

        [JsonProperty]
        public string Subproject { get; set; }

        [JsonProperty]
        public string VendorId { get; set; }

        [JsonProperty]
        public string ProductId { get; set; }

        [JsonProperty]
        public string DeviceVer { get; set; }

        [JsonProperty]
        public string ChipBase { get; set; }

        [JsonProperty]
        public string Mcu { get; set; }
    }
}
