﻿//------------------------------------------------------------------------------
// <copyright file="AzureKinectWriteCustomTrackDataException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Kinect.Sensor.Record.Exceptions
{
    /// <summary>
    /// Represents errors that occur when writing a custom track.
    /// </summary>
    [Serializable]
    public class AzureKinectWriteCustomTrackDataException : AzureKinectRecordException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKinectWriteCustomTrackDataException"/> class.
        /// </summary>
        public AzureKinectWriteCustomTrackDataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKinectWriteCustomTrackDataException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AzureKinectWriteCustomTrackDataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKinectWriteCustomTrackDataException"/> class
        /// with a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference
        /// (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public AzureKinectWriteCustomTrackDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKinectWriteCustomTrackDataException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about the
        /// exception being thrown.</param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/>System.Runtime.Serialization.StreamingContext that
        /// contains contextual information about the source or destination.
        /// </param>
        protected AzureKinectWriteCustomTrackDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKinectWriteCustomTrackDataException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="logMessages">
        /// The log messages that happened during the function call that generated this error.
        /// </param>
        protected AzureKinectWriteCustomTrackDataException(string message, ICollection<LogMessage> logMessages)
            : base(message, logMessages)
        {
        }

        /// <summary>
        /// Throws an <see cref="AzureKinectWriteCustomTrackDataException"/> if the result of the function
        /// is not a success.
        /// </summary>
        /// <param name="function">The native function to call.</param>
        /// <typeparam name="T">The type of result to expect from the function call.</typeparam>
        internal static void ThrowIfNotSuccess<T>(Func<T> function)
            where T : System.Enum
        {
            using (LoggingTracer tracer = new LoggingTracer())
            {
                T result = function();
                if (!AzureKinectRecordException.IsSuccess(result))
                {
                    throw new AzureKinectWriteCustomTrackDataException($"result = {result}", tracer.LogMessages);
                }
            }
        }
    }
}
