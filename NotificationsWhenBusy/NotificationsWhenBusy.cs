using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using BaseX;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System;
using System.Reflection;

namespace NotificationsWhenBusy
{
    public class NotificationsWhenBusy : NeosMod
    {
        public override string Name => "NotificationsWhenBusy";
        public override string Author => "DoubleStyx";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/DoubleStyx/NotificationsWhenBusy";

        private static ModConfiguration _config;
        private static Harmony _harmony;
        private static MethodInfo _addNotification;
        private static HarmonyMethod _transpiler;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> allowNotificationsWhenBusy = new ModConfigurationKey<bool>("Allow notifications when busy", "", () => true);

        public override void OnEngineInit()
        {
            _harmony = new Harmony("net.DoubleStyx.NotificationsWhenBusy");

            // Config and unpatching not working as of v1.0.0; needs fixing

            /*
            _config = GetConfiguration();
            ModConfiguration.OnAnyConfigurationChanged += OnConfigurationChanged;
            */

            _transpiler = new HarmonyMethod(typeof(AddNotification_Patch), nameof(AddNotification_Patch.Transpiler));
            _addNotification = AccessTools.Method(typeof(NotificationPanel), "AddNotification",
                new Type[] { typeof(string), typeof(string), typeof(Uri), typeof(color), typeof(string), typeof(Uri), typeof(IAssetProvider<AudioClip>) });
            
            _harmony.Patch(_addNotification, transpiler: _transpiler);
        }

        private void OnConfigurationChanged(ConfigurationChangedEvent @event)
        {
            if (_config.GetValue(allowNotificationsWhenBusy))
            {
                _harmony.Patch(_addNotification, transpiler: _transpiler);
            }
            else
            {
                _harmony.Unpatch(_addNotification, HarmonyPatchType.Transpiler);
            }
        }

        public static class AddNotification_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ret) // If found first return
                    {
                        codes[i].opcode = OpCodes.Nop; // Don't return
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}
