using System;

namespace SharpNoise.Modules
{
    /// <summary>
    /// Noise module that moves the coordinates of the input value before
    /// returning the output value from a source module.
    /// </summary>
    /// <remarks>
    /// The <see cref="GetValue"/> method moves the ( x, y, z ) coordinates of
    /// the input value by a translation amount before returning the output
    /// value from the source module.  To set the translation amount, call
    /// the <see cref="SetTranslation"/> method.  To set the translation amount to
    /// apply to the individual x, y, or z coordinates, modify the
    /// XTranslation, YTranslation or ZTranslation properties,
    /// respectively.
    ///
    /// This noise module requires one source module.
    /// </remarks>
    [Serializable]
    public class TranslatePoint : Module
    {
        /// <summary>
        /// Default translation amount
        /// </summary>
        public const double DefaultTranslation = 0D;

        /// <summary>
        /// Gets or sets the first source module
        /// </summary>
        public Module Source0
        {
            get { return this.SourceModules[0]; }
            set { this.SourceModules[0] = value; }
        }

        /// <summary>
        /// Gets or sets the translation amount to apply to theb x coordinate of
        /// the input value.
        /// </summary>
        public double XTranslation { get; set; } = DefaultTranslation;

        /// <summary>
        /// Gets or sets the translation amount to apply to theb y coordinate of
        /// the input value.
        /// </summary>
        public double YTranslation { get; set; } = DefaultTranslation;

        /// <summary>
        /// Gets or sets the translation amount to apply to the z coordinate of
        /// the input value.
        /// </summary>
        public double ZTranslation { get; set; } = DefaultTranslation;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TranslatePoint()
            : base(1)
        {
        }

        /// <summary>
        /// Sets the translation value to apply to the input value.
        /// </summary>
        /// <param name="translation">The translation value to apply.</param>
        /// <remarks>
        /// The <see cref="GetValue"/> method moves the ( x, y, z ) coordinates
        /// of the input value by a translation amount before returning the
        /// output value from the source module
        /// </remarks>
        public void SetTranslation(double translation)
        {
            this.XTranslation = translation;
            this.YTranslation = translation;
            this.ZTranslation = translation;
        }

        /// <summary>
        /// Sets the translation values to apply to the input value.
        /// </summary>
        /// <param name="xTranslation">Translation value to apply to the X coordinate.</param>
        /// <param name="yTranslation">Translation value to apply to the Y coordinate.</param>
        /// <param name="zTranslation">Translation value to apply to the Z coordinate.</param>
        /// <remarks>
        /// The <see cref="GetValue"/> method moves the ( x, y, z ) coordinates
        /// of the input value by a translation amount before returning the
        /// output value from the source module
        /// </remarks>
        public void SetTranslation(double xTranslation, double yTranslation, double zTranslation)
        {
            this.XTranslation = xTranslation;
            this.YTranslation = yTranslation;
            this.ZTranslation = zTranslation;
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
            return this.SourceModules[0].GetValue(
                x + this.XTranslation,
                y + this.YTranslation,
                z + this.ZTranslation);
        }
    }
}
