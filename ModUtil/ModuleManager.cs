using System.Collections.Generic;
using MelonLoader;

namespace ModUtil
{
    internal class ModuleManager
    {
        internal static Dictionary<string, Module> modules = new Dictionary<string, Module>();
        private string mod_id;

        public ModuleManager(string mod_id)
        {
            this.mod_id = mod_id;
        }

        public void Add(string id, Module module)
        {
            modules.Add(id, module);
        }

        public void UnloadAllDynamicAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool dynamic_unloaded = module.TryUnloadDynamicAssets();

                if (dynamic_unloaded)
                {
                    MelonLogger.Msg(mod_id + " dynamic assets unloaded from module: " + id);
                }
            }
        }

        public void LoadAllDynamicAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool loaded = module.TryLoadDynamicAssets();

                if (loaded)
                {
                    MelonLogger.Msg(mod_id + " dynamic assets loaded from module: " + id);
                }
            }
        }

        public void LoadAllStaticAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool static_loaded = module.TryLoadStaticAssets();

                if (static_loaded)
                {
                    MelonLogger.Msg(mod_id + " static assets loaded from module: " + id);
                }
            }
        }
    }
}
