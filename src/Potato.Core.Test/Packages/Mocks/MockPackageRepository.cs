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
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace Potato.Core.Test.Packages.Mocks {
    public class MockPackageRepository : PackageRepositoryBase {

        protected IEnumerable<IPackage> Packages { get; set; }

        public MockPackageRepository() {
            this.Packages = new List<IPackage>();
        }

        public MockPackageRepository(IEnumerable<IPackage> packages) {
            this.Packages = packages;
        }

        public override IQueryable<IPackage> GetPackages() {
            return this.Packages.AsQueryable();
        }

        public override string Source {
            get { return ""; }
        }

        public override bool SupportsPrereleasePackages {
            get { return false; }
        }
    }
}
