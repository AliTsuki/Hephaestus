using System;

namespace SharpNoise.Modules
{
    /// <summary>
    /// Noise module that randomly displaces the input value before
    /// returning the output value from a source module.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Turbulence is the pseudo-random displacement of the input value.
    /// The GetValue() method randomly displaces the ( x, y, z )
    /// coordinates of the input value before retrieving the output value from
    /// the source module.  To control the turbulence, an application can
    /// modify its frequency, its power, and its roughness.
    /// </para>
    /// 
    /// <para>
    /// The frequency of the turbulence determines how rapidly the
    /// displacement amount changes.  To specify the frequency, modify the
    /// <see cref="Frequency"/> property.
    /// </para>
    /// 
    /// <para>
    /// The power of the turbulence determines the scaling factor that is
    /// applied to the displacement amount.  To specify the power, modify the
    /// <see cref="Power"/> property.
    /// </para>
    /// 
    /// <para>
    /// The roughness of the turbulence determines the roughness of the
    /// changes to the displacement amount.  Low values smoothly change the
    /// displacement amount.  High values roughly change the displacement
    /// amount, which produces more "kinky" changes.  To specify the
    /// roughness, modify the <see cref="Roughness"/> property.
    /// </para>
    /// 
    /// <para>
    /// Use of this noise module may require some trial and error.  Assuming
    /// that you are using a generator module as the source module, you
    /// should first:
    /// <list type="bullet">
    /// <item>
    /// <description>Set the frequency to the same frequency as the source module.</description>
    /// </item>
    /// <item>
    /// <description>Set the power to the reciprocal of the frequency.</description>
    /// </item>
    /// </list>
    ///
    /// From these initial frequency and power values, modify these values
    /// until this noise module produce the desired changes in your terrain or
    /// texture.  For example:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Low frequency (1/8 initial frequency) and low power (1/8 initial
    /// power) produces very minor, almost unnoticeable changes.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Low frequency (1/8 initial frequency) and high power (8 times
    /// initial power) produces "ropey" lava-like terrain or marble-like
    /// textures.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// High frequency (8 times initial frequency) and low power (1/8
    /// initial power) produces a noisy version of the initial terrain or
    /// texture.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// High frequency (8 times initial frequency) and high power (8 times
    /// initial power) produces nearly pure noise, which isn't entirely
    /// useful.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// Displacing the input values result in more realistic terrain and
    /// textures.  If you are generating elevations for terrain height maps,
    /// you can use this noise module to produce more realistic mountain
    /// ranges or terrain features that look like flowing lava rock.  If you
    /// are generating values for textures, you can use this noise module to
    /// produce realistic marble-like or "oily" textures.
    ///
    /// Internally, there are three Perlin noise modules
    /// that displace the input value; one for the x, one for the y,
    /// and one for the z coordinate.
    /// </para>
    /// 
    /// This noise module requires one source module.
    /// </remarks>
    [Serializable]
    public class Turbulence : Module
    {
        /// <summary>
        /// Default frequency
        /// </summary>
        public const double DefaultFrequency = Perlin.DefaultFrequency;

        /// <summary>
        /// Default power
        /// </summary>
        public const double DefaultPower = 1D;

        /// <summary>
        /// Default roughness
        /// </summary>
        public const int DefaultRoughness = 3;

        /// <summary>
        /// Default noise seed
        /// </summary>
        public const int DefaultSeed = Perlin.DefaultSeed;

        /// <summary>
        /// Gets or sets the first source module
        /// </summary>
        public Module Source0
        {
            get { return this.SourceModules[0]; }
            set { this.SourceModules[0] = value; }
        }

        /// <summary>
        /// Gets or sets the power of the turbulence.
        /// </summary>
        /// <remarks>
        /// The power of the turbulence determines the scaling factor that is
        /// applied to the displacement amount.
        /// </remarks>
        public double Power { get; set; } = DefaultPower;

        /// <summary>
        /// Gets or sets the frequency of the turbulence.
        /// </summary>
        /// <remarks>
        /// The frequency of the turbulence determines how rapidly the
        /// displacement amount changes.
        /// </remarks>
        public double Frequency
        {
            get
            {
                return this.xDistort.Frequency;
            }
            set
            {
                this.xDistort.Frequency = value;
                this.yDistort.Frequency = value;
                this.zDistort.Frequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the roughness of the turbulence.
        /// </summary>
        /// <remarks>
        /// The roughness of the turbulence determines the roughness of the
        /// changes to the displacement amount.  Low values smoothly change
        /// the displacement amount.  High values roughly change the
        /// displacement amount, which produces more "kinky" changes.
        /// </remarks>
        public int Roughness
        {
            get
            {
                return this.xDistort.OctaveCount;
            }
            set
            {
                this.xDistort.OctaveCount = value;
                this.yDistort.OctaveCount = value;
                this.zDistort.OctaveCount = value;
            }
        }

        /// <summary>
        /// Returns the seed value of the internal Perlin-noise modules that
        /// are used to displace the input values.
        /// </summary>
        /// <remarks>
        /// Internally, there are three Perlin noise modules
        /// that displace the input value; one for the x, one for the y,
        /// and one for the z coordinate.  
        /// </remarks>
        public int Seed
        {
            get
            {
                return this.xDistort.Seed;
            }
            set
            {
                this.xDistort.Seed = value;
                this.yDistort.Seed = value + 1;
                this.zDistort.Seed = value + 2;
            }
        }

        private readonly Perlin xDistort, yDistort, zDistort;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Turbulence()
            : base(1)
        {
            this.xDistort = new Perlin();
            this.yDistort = new Perlin();
            this.zDistort = new Perlin();
            this.Seed = DefaultSeed;
            this.Frequency = DefaultFrequency;
            this.Roughness = DefaultRoughness;
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
            // Get the values from the three Perlin noise modules and
            // add each value to each coordinate of the input value. There are also
            // some offsets added to the coordinates of the input values. This prevents
            // the distortion modules from returning zero if the (x, y, z) coordinates,
            // when multiplied by the frequency, are near an integer boundary. This is
            // due to a property of gradient coherent noise, which returns zero at
            // integer boundaries.
            double x0, y0, z0;
            double x1, y1, z1;
            double x2, y2, z2;
            x0 = x + (12414D / 65536D);
            y0 = y + (65124D / 65536D);
            z0 = z + (31337D / 65536D);
            x1 = x + (26519D / 65536D);
            y1 = y + (18128D / 65536D);
            z1 = z + (60493D / 65536D);
            x2 = x + (53820D / 65536D);
            y2 = y + (11213D / 65536D);
            z2 = z + (44845D / 65536D);
            double xDistorted = x + (this.xDistort.GetValue(x0, y0, z0) * this.Power);
            double yDistorted = y + (this.yDistort.GetValue(x1, y1, z1) * this.Power);
            double zDistorted = z + (this.zDistort.GetValue(x2, y2, z2) * this.Power);

            // Retrieve the output value at the offsetted input value instead of the
            // original input value.
            return this.SourceModules[0].GetValue(xDistorted, yDistorted, zDistorted);
        }
    }
}
