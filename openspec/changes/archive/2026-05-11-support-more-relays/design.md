# Design: Extending Relay Support  
## Changes:  
1. Update `SetRelayState` method to include relays 5-8.  
   - Add `SetRelay` calls with byte addressing:  
     - Relay 5: `0x05`  
     - Relay 6: `0x06`  
     - Relay 7: `0x07`  
     - Relay 8: `0x08`  
2. Modify `appsettings.json` files to reflect the extended support:  
   - Update the line `1,2,3,4` to `1,2,3,4,5,6,7,8` in:  
     - `appsettings.json`  
     - `appsettings.Development.json`  
     - `appsettings.Container.json`  