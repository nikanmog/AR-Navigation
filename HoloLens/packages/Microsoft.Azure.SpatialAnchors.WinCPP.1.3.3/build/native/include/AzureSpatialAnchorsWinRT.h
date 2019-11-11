// Copyright (c) Microsoft Corporation. All rights reserved.

#pragma once

#include <unknwn.h>
#include "winrt/Microsoft.Azure.SpatialAnchors.h"

extern "C" {
    extern HRESULT WINAPI ASACreateFactory(
        IUnknown** ppFactory);
}
