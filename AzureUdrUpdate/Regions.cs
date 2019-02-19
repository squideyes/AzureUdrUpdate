// Copyright 2019 Louis S. Berman
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;

namespace AzureUdrUpdate
{
    public static class Regions
    {
        private static HashSet<string> regions = new HashSet<string>();

        static Regions()
        {
            regions.Add("canadacentral");
            regions.Add("canadaeast");
            regions.Add("uscentral");
            regions.Add("uscentraleuap");
            regions.Add("useast");
            regions.Add("useast2");
            regions.Add("useast2euap");
            regions.Add("usnorth");
            regions.Add("ussouth");
            regions.Add("uswest");
            regions.Add("uswest2");
            regions.Add("uswestcentral");
        }

        public static bool IsValid(string region) => 
            regions.Contains(region);
    }
}
