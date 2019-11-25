﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class XRUXPickerForLauncher : XRUXPicker
    {
        private static XRUXPickerForLauncher _Instance;
        public new static XRUXPickerForLauncher Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<XRUXPickerForLauncher>();
                }

                return _Instance;
            }
        }

    }
}
