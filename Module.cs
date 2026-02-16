namespace PactIncreasedLethality
{
    public class Module {
        private bool static_assets_loaded = false;
        private bool dynamic_assets_loaded = false;

        public bool TryLoadStaticAssets()
        {
            if (static_assets_loaded) return false;

            LoadStaticAssets();

            static_assets_loaded = true;

            return true;
        }

        public bool TryLoadDynamicAssets()
        {
            if (dynamic_assets_loaded) return false;

            LoadDynamicAssets();

            dynamic_assets_loaded = true;

            return true;
        }

        public bool TryUnloadDynamicAssets()
        {
            if (!dynamic_assets_loaded) return false;

            dynamic_assets_loaded = false;

            UnloadDynamicAssets();

            return true;
        }

        public virtual void LoadStaticAssets() {}
        public virtual void LoadDynamicAssets() {}
        public virtual void UnloadDynamicAssets() {}
    }
}
