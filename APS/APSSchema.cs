namespace ActiveProtectionSystem
{
    internal class APSSchema
    {
        public int min_engage_caliber = 70;
        public float min_engage_velocity = 70f;
        public float max_engage_velocity = 700f;
        public float intercept_delay = 2.0f;

        public APSSchema() { }

        public APSSchema(int min_engage_caliber, float min_engage_velocity, float max_engage_velocity, float intercept_delay)
        {
            this.min_engage_caliber = min_engage_caliber;
            this.min_engage_velocity = min_engage_velocity;
            this.max_engage_velocity = max_engage_velocity;
            this.intercept_delay = intercept_delay;
        }
    }
}
