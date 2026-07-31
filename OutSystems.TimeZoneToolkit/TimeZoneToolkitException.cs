using System;

namespace OutSystems.TimeZoneToolkit
{
    /// <summary>
    /// Raised when an action receives input it cannot process, for example an unknown time zone
    /// identifier or a date and time that does not exist because of a daylight saving transition.
    /// The message is surfaced to the OutSystems app and can be caught with an Exception Handler.
    /// </summary>
    public class TimeZoneToolkitException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="TimeZoneToolkitException"/>.
        /// </summary>
        /// <param name="message">The validation message shown to the OutSystems developer.</param>
        public TimeZoneToolkitException(string message) : base(message)
        {
        }
    }
}
