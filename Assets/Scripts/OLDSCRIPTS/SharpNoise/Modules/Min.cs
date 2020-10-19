using System;
namespace SharpNoise.Modules
{
    /// <summary>
    /// Noise module that outputs the smaller of the two output values from two
    /// source modules.
    /// </summary>
    /// <remarks>
    /// This noise module requires two source modules.
    /// </remarks>
    [Serializable]
    public class Min : Module
    {
        /// <summary>
        /// Gets or sets the first source module
        /// </summary>
        public Module Source0
        {
            get { return this.SourceModules[0]; }
            set { this.SourceModules[0] = value; }
        }

        /// <summary>
        /// Gets or sets the second source module
        /// </summary>
        public Module Source1
        {
            get { return this.SourceModules[1]; }
            set { this.SourceModules[1] = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Min()
            : base(2)
        {

        }

        /// <summary>
        /// See the documentation on the base class.
        /// <seealso cref="Module"/>
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>Returns the computed value</returns>
        public override double GetValue(double x, double y, double z)
        {
            return Math.Min(
                this.SourceModules[0].GetValue(x, y, z),
                this.SourceModules[1].GetValue(x, y, z));
        }
    }
}
