﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.HAL
{
    public static class Global
    {
        public static readonly Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Hardware", "");

        public static Keyboard Keyboard;

        public static bool NumLock
        {
            get { return _numLock; }
            set { _numLock = value; Keyboard?.UpdateLeds(); }
        }

        public static bool CapsLock
        {
            get { return _capsLock; }
            set { _capsLock = value; Keyboard?.UpdateLeds(); }
        }

        public static bool ScrollLock
        {
            get { return _scrollLock; }
            set
            {
                _scrollLock = value;
                Keyboard?.UpdateLeds();
            }
        }

        //static public PIT PIT = new PIT();
        // Must be static init, other static inits rely on it not being null
        public static TextScreenBase TextScreen = new TextScreen();

        public static PCI Pci;
        private static bool _numLock;
        private static bool _capsLock;
        private static bool _scrollLock;

        private static void InitAta(BlockDevice.Ata.ControllerIdEnum aControllerID,
            BlockDevice.Ata.BusPositionEnum aBusPosition)
        {
            var xIO = aControllerID == BlockDevice.Ata.ControllerIdEnum.Primary
                ? Cosmos.Core.Global.BaseIOGroups.ATA1
                : Cosmos.Core.Global.BaseIOGroups.ATA2;
            var xATA = new BlockDevice.AtaPio(xIO, aControllerID, aBusPosition);
            if (xATA.DriveType == BlockDevice.AtaPio.SpecLevel.Null)
            {
                return;
            }
            if (xATA.DriveType == BlockDevice.AtaPio.SpecLevel.ATA)
            {
                BlockDevice.BlockDevice.Devices.Add(xATA);
                Ata.AtaDebugger.Send("ATA device with speclevel ATA found.");
            }
            else
            {
                Ata.AtaDebugger.Send("ATA device with spec level " + (int) xATA.DriveType +
                                     " found, which is not supported!");
                return;
            }
            var xMbrData = new byte[512];
            xATA.ReadBlock(0UL, 1U, xMbrData);
            var xMBR = new BlockDevice.MBR(xMbrData);

            if (xMBR.EBRLocation != 0)
            {
                //EBR Detected
                var xEbrData = new byte[512];
                xATA.ReadBlock(xMBR.EBRLocation, 1U, xEbrData);
                var xEBR = new BlockDevice.EBR(xEbrData);

                for (int i = 0; i < xEBR.Partitions.Count; i++)
                {
                    //var xPart = xEBR.Partitions[i];
                    //var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                    //BlockDevice.BlockDevice.Devices.Add(xPartDevice);
                }
            }

            // TODO Change this to foreach when foreach is supported
            Ata.AtaDebugger.Send("Number of MBR partitions found:  " + xMBR.Partitions.Count);
            for (int i = 0; i < xMBR.Partitions.Count; i++)
            {
                var xPart = xMBR.Partitions[i];
                if (xPart == null)
                {
                    Console.WriteLine("Null partition found at idx " + i);
                }
                else
                {
                    var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                    BlockDevice.BlockDevice.Devices.Add(xPartDevice);
                    Console.WriteLine("Found partition at idx " + i);
                }
            }
        }

        // Init devices that are "static"/mostly static. These are devices
        // that all PCs are expected to have. Keyboards, screens, ATA hard drives etc.
        // Despite them being static, some discovery is required. For example, to see if
        // a hard drive is connected or not and if so what type.
        internal static void InitStaticDevices()
        {
            //TextScreen = new TextScreen();
            Global.Dbg.Send("CLS");
      //TODO: Since this is FCL, its "common". Otherwise it should be
      // system level and not accessible from Core. Need to think about this
      // for the future.
      Debugger.DoSend("Finding PCI Devices");
      //PCI.Setup();
    }

    static public void Init(TextScreenBase textScreen, Keyboard keyboard)
    {
      if (textScreen != null)
      {
        TextScreen = textScreen;
      }
        if (keyboard == null)
        {
            Core.Global.Dbg.Send("No keyboard specified!");
            Keyboard = new PS2Keyboard();
        }
        else
        {
            Keyboard = keyboard;
        }
        Global.Dbg.Send("Before Core.Global.Init");
      Core.Global.Init();
      Global.Dbg.Send("Static Devices");
      InitStaticDevices();
      Global.Dbg.Send("PCI Devices");
      InitPciDevices();
      Global.Dbg.Send("Done initializing Cosmos.HAL.Global");

            Global.Dbg.Send("ATA Primary Master");
            InitAta(BlockDevice.Ata.ControllerIdEnum.Primary, BlockDevice.Ata.BusPositionEnum.Master);

            //TODO Need to change code to detect if ATA controllers are present or not. How to do this? via PCI enum?
            // They do show up in PCI space as well as the fixed space.
            // Or is it always here, and was our compiler stack corruption issue?
            Global.Dbg.Send("ATA Secondary Master");
            InitAta(BlockDevice.Ata.ControllerIdEnum.Secondary, BlockDevice.Ata.BusPositionEnum.Master);
            //InitAta(BlockDevice.Ata.ControllerIdEnum.Secondary, BlockDevice.Ata.BusPositionEnum.Slave);
        }

        internal static void InitPciDevices()
        {
            //TODO Redo this - Global init should be other.
            // Move PCI detection to hardware? Or leave it in core? Is Core PC specific, or deeper?
            // If we let hardware do it, we need to protect it from being used by System.
            // Probably belongs in hardware, and core is more specific stuff like CPU, memory, etc.
            //Core.PCI.OnPCIDeviceFound = PCIDeviceFound;

            //TODO: Since this is FCL, its "common". Otherwise it should be
            // system level and not accessible from Core. Need to think about this
            // for the future.
            Console.WriteLine("Finding PCI Devices");
            PCI.Setup();
        }
        
    public static void EnableInterrupts()
    {
      CPU.EnableInterrupts();
    }

    public static bool InterruptsEnabled
    {
      get
      {
        return CPU.mInterruptsEnabled;
      }
    }
  }
}
