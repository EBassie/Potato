﻿#region Copyright
// Copyright 2014 Myrcon Pty. Ltd.
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
#endregion
using NuGet;

namespace Procon.Service.Shared.Packages {
    /// <summary>
    /// Dispatches simple requests to the manager to complete actions.
    /// </summary>
    public class PackageManagerDispatch : IPackageManagerDispatch {
        public void InstallPackage(IPackageManager manager, IPackage package) {
            manager.InstallPackage(package, false, false);
        }

        public void UpdatePackage(IPackageManager manager, IPackage package) {
            manager.UpdatePackage(package, true, false);
        }

        public void UninstallPackage(IPackageManager manager, IPackage package) {
            manager.UninstallPackage(package, false, true);
        }
    }
}
