// --------------------------------------------------------------------------------------------
// <copyright file="MalformedCsvException.cs" company="Effort Team">
//     Copyright (C) Effort Team
//     Copyright (C) 2006 Sébastien Lorion
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Csv
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    ///     Represents the exception that is thrown when a CSV file is malformed.
    /// </summary>
    [Serializable]
    internal class MalformedCsvException
        : Exception
    {
        #region Fields

        /// <summary>
        ///     Contains the message that describes the error.
        /// </summary>
        private string message;

        /// <summary>
        ///     Contains the raw data when the error occured.
        /// </summary>
        private string rawData;

        /// <summary>
        ///     Contains the current field index.
        /// </summary>
        private int currentFieldIndex;

        /// <summary>
        ///     Contains the current record index.
        /// </summary>
        private long currentRecordIndex;

        /// <summary>
        ///     Contains the current position in the raw data.
        /// </summary>
        private int currentPosition;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the MalformedCsvException class.
        /// </summary>
        public MalformedCsvException()
            : this(null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the MalformedCsvException class.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        public MalformedCsvException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the MalformedCsvException class.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception.
        /// </param>
        public MalformedCsvException(string message, Exception innerException)
            : base(string.Empty, innerException)
        {
            this.message = message is null ? string.Empty : message;

            rawData = string.Empty;
            currentPosition = -1;
            currentRecordIndex = -1;
            currentFieldIndex = -1;
        }

        /// <summary>
        ///     Initializes a new instance of the MalformedCsvException class.
        /// </summary>
        /// <param name="rawData">
        ///     The raw data when the error occured.
        /// </param>
        /// <param name="currentPosition">
        ///     The current position in the raw data.
        /// </param>
        /// <param name="currentRecordIndex">
        ///     The current record index.
        /// </param>
        /// <param name="currentFieldIndex">
        ///     The current field index.
        /// </param>
        public MalformedCsvException(
            string rawData,
            int currentPosition,
            long currentRecordIndex,
            int currentFieldIndex)
            : this(rawData, currentPosition, currentRecordIndex, currentFieldIndex, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the MalformedCsvException class.
        /// </summary>
        /// <param name="rawData">
        ///     The raw data when the error occured.
        /// </param>
        /// <param name="currentPosition">
        ///     The current position in the raw data.
        /// </param>
        /// <param name="currentRecordIndex">
        ///     The current record index.
        /// </param>
        /// <param name="currentFieldIndex">
        ///     The current field index.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception.
        /// </param>
        public MalformedCsvException(
            string rawData,
            int currentPosition,
            long currentRecordIndex,
            int currentFieldIndex,
            Exception innerException)
            : base(string.Empty, innerException)
        {
            this.rawData = rawData is null ? string.Empty : rawData;
            this.currentPosition = currentPosition;
            this.currentRecordIndex = currentRecordIndex;
            this.currentFieldIndex = currentFieldIndex;

            message =
                string.Format(
                    CultureInfo.InvariantCulture,
                    ExceptionMessages.MalformedCsvException,
                    this.currentRecordIndex,
                    this.currentFieldIndex,
                    this.currentPosition,
                    this.rawData);
        }

        /// <summary>
        ///     Initializes a new instance of the MalformedCsvException class with serialized 
        ///     data.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:SerializationInfo"/> that holds the serialized object data
        ///     about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:StreamingContext"/> that contains contextual information about
        ///     the source or destination.
        /// </param>
        protected MalformedCsvException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            message = info.GetString("MyMessage");

            rawData = info.GetString("RawData");
            currentPosition = info.GetInt32("CurrentPosition");
            currentRecordIndex = info.GetInt64("CurrentRecordIndex");
            currentFieldIndex = info.GetInt32("CurrentFieldIndex");
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the raw data when the error occured.
        /// </summary>
        /// <value> The raw data when the error occured. </value>
        public string RawData
        {
            get { return rawData; }
        }

        /// <summary>
        ///     Gets the current position in the raw data.
        /// </summary>
        /// <value> The current position in the raw data. </value>
        public int CurrentPosition
        {
            get { return currentPosition; }
        }

        /// <summary>
        ///     Gets the current record index.
        /// </summary>
        /// <value> The current record index. </value>
        public long CurrentRecordIndex
        {
            get { return currentRecordIndex; }
        }

        /// <summary>
        ///     Gets the current field index.
        /// </summary>
        /// <value> The current record index. </value>
        public int CurrentFieldIndex
        {
            get { return currentFieldIndex; }
        }

        #endregion

        #region Overrides

        /// <summary>
        ///     Gets a message that describes the current exception.
        /// </summary>
        /// <value> A message that describes the current exception. </value>
        public override string Message
        {
            get { return message; }
        }

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="T:SerializationInfo"/>
        ///     with information about the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:SerializationInfo"/> that holds the serialized object data
        ///     about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:StreamingContext"/> that contains contextual information about
        ///     the source or destination.
        /// </param>
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("MyMessage", message);

            info.AddValue("RawData", rawData);
            info.AddValue("CurrentPosition", currentPosition);
            info.AddValue("CurrentRecordIndex", currentRecordIndex);
            info.AddValue("CurrentFieldIndex", currentFieldIndex);
        }

        #endregion
    }
}