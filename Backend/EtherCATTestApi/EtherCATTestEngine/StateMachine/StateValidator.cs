﻿using System;
using System.Threading.Tasks;

namespace EtherCATTestApi.EtherCATTestEngine.StateMachine
{
    public enum EtherCATState
    {
        Init = 0,
        PreOperational = 1,
        SafeOperational = 2,
        Operational = 3,
        Boot = 4
    }
    
    public class StateValidator
    {
        private readonly EtherCATCommManager _commManager;
        
        public StateValidator(EtherCATCommManager commManager)
        {
            _commManager = commManager;
        }
        
        public async Task<bool> ValidateStateTransitionAsync(EtherCATState fromState, EtherCATState toState)
        {
            if (!_commManager.IsConnected)
            {
                throw new InvalidOperationException("Not connected to EtherCAT device");
            }
            
            Console.WriteLine($"Testing transition from {fromState} to {toState}");
            
            // Send state transition command
            var command = BuildStateTransitionCommand(fromState, toState);
            var response = await _commManager.SendCommandAsync(command);
            
            // Validate response
            return ValidateStateTransitionResponse(response, toState);
        }
        
        private byte[] BuildStateTransitionCommand(EtherCATState fromState, EtherCATState toState)
        {
            // Simplified command building - actual implementation would follow EtherCAT protocol
            var command = new byte[16];
            command[0] = 0x01; // Command type: State Transition
            command[1] = (byte)fromState;
            command[2] = (byte)toState;
            return command;
        }
        
        private bool ValidateStateTransitionResponse(byte[] response, EtherCATState expectedState)
        {
            // Simplified response validation
            if (response.Length < 4) return false;
            return response[3] == (byte)expectedState;
        }
        
        public async Task<bool> ValidateFullStateSequenceAsync()
        {
            // Test complete state sequence: Init → Pre-Oper → Safe-Oper → Oper
            bool success = true;
            
            success &= await ValidateStateTransitionAsync(EtherCATState.Init, EtherCATState.PreOperational);
            success &= await ValidateStateTransitionAsync(EtherCATState.PreOperational, EtherCATState.SafeOperational);
            success &= await ValidateStateTransitionAsync(EtherCATState.SafeOperational, EtherCATState.Operational);
            
            // Return to Init for cleanup
            await ValidateStateTransitionAsync(EtherCATState.Operational, EtherCATState.Init);
            
            return success;
        }
    }
}