﻿// Copyright 2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Newtonsoft.Json.Linq;
using Sensus.Encryption;
using Sensus.Exceptions;
using Sensus.Extensions;

namespace Sensus.Authentication
{
    /// <summary>
    /// Handles all interactions with an [authentication server](xref:authentication_servers).
    /// </summary>
    public class AuthenticationService : IEnvelopeEncryptor
    {
        private const string CREATE_ACCOUNT_PATH = "/createaccount?deviceId={0}&participantId={1}";
        private const string GET_CREDENTIALS_PATH = "/getcredentials?participantId={0}&password={1}";

        private readonly string _createAccountURL;
        private readonly string _getCredentialsURL;
        
        /// <summary>
        /// Gets or sets the base service URL.
        /// </summary>
        /// <value>The base service URL.</value>
        public string BaseServiceURL { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>The account.</value>
        public Account Account { get; set; }

        /// <summary>
        /// Gets or sets the AWS S3 credentials. 
        /// </summary>
        /// <value>The credentials.</value>
        public AmazonS3Credentials AmazonS3Credentials { get; set; }

        public Protocol Protocol { get; set; }

        public AuthenticationService(string baseServiceURL)
        {
            BaseServiceURL = baseServiceURL.Trim('/');

            _createAccountURL = BaseServiceURL + CREATE_ACCOUNT_PATH;
            _getCredentialsURL = BaseServiceURL + GET_CREDENTIALS_PATH;
        }

        public async Task<Account> CreateAccountAsync(string participantId = null)
        {
            string accountJSON = await new Uri(string.Format(_createAccountURL, SensusServiceHelper.Get().DeviceId, participantId)).DownloadString();

            try
            {
                Account = accountJSON.DeserializeJson<Account>();
            }
            catch (Exception ex)
            {
                SensusException.Report("Exception while deserializing account:  " + ex.Message, ex);
            }

            // check properties

            if (string.IsNullOrWhiteSpace(Account.ParticipantId))
            {
                SensusException.Report("Empty " + nameof(Account.ParticipantId) + " returned by authentication service for device " + SensusServiceHelper.Get().DeviceId + " and participant " + (participantId ?? "[null]."));
            }

            if (string.IsNullOrWhiteSpace(Account.Password))
            {
                SensusException.Report("Empty " + nameof(Account.Password) + " returned by authentication service for device " + SensusServiceHelper.Get().DeviceId + " and participant " + (participantId ?? "[null]."));
            }

            // save the app state to hang on to the account
            await SensusServiceHelper.Get().SaveAsync();

            return Account;
        }

        public async Task<AmazonS3Credentials> GetCredentialsAsync()
        {
            // create account if we don't have one for some reason. under normal conditions we can expect to always have an account, 
            // as the account information is downloaded, attached to the protocol, and saved with the protocol when the protocol is started.
            if (Account == null)
            {
                await CreateAccountAsync();
            }

            string credentialsJSON = await new Uri(string.Format(_getCredentialsURL, Account.ParticipantId, Account.Password)).DownloadString();

            // create a new account if the password was bad. this also should not be possible.
            if (IsExceptionResponse(credentialsJSON))
            {
                await CreateAccountAsync();

                credentialsJSON = await new Uri(string.Format(_getCredentialsURL, Account.ParticipantId, Account.Password)).DownloadString();

                if (IsExceptionResponse(credentialsJSON))
                {
                    SensusException.Report("Received bad password response when getting credentials with newly created account.");
                    throw new NotImplementedException();
                }
            }

            // deserialize credentials
            try
            {
                AmazonS3Credentials = credentialsJSON.DeserializeJson<AmazonS3Credentials>();
            }
            catch (Exception ex)
            {
                SensusException.Report("Exception while deserializing AWS S3 credentials.", ex);
                throw ex;
            }

            // check properties

            if (string.IsNullOrWhiteSpace(AmazonS3Credentials.AccessKeyId))
            {
                SensusException.Report("Empty " + nameof(AmazonS3Credentials.AccessKeyId) + " returned by authentication service for participant " + (Account.ParticipantId ?? "[null]."));
            }

            if (string.IsNullOrWhiteSpace(AmazonS3Credentials.CustomerMasterKey))
            {
                SensusException.Report("Empty " + nameof(AmazonS3Credentials.CustomerMasterKey) + " returned by authentication service for participant " + (Account.ParticipantId ?? "[null]."));
            }

            if (string.IsNullOrWhiteSpace(AmazonS3Credentials.ExpirationUnixTimeMilliseconds))
            {
                SensusException.Report("Empty " + nameof(AmazonS3Credentials.ExpirationUnixTimeMilliseconds) + " returned by authentication service for participant " + (Account.ParticipantId ?? "[null]."));
            }

            if (string.IsNullOrWhiteSpace(AmazonS3Credentials.ProtocolId))
            {
                SensusException.Report("Empty " + nameof(AmazonS3Credentials.ProtocolId) + " returned by authentication service for participant " + (Account.ParticipantId ?? "[null]."));
            }

            if (string.IsNullOrWhiteSpace(AmazonS3Credentials.ProtocolURL))
            {
                SensusException.Report("Empty " + nameof(AmazonS3Credentials.ProtocolURL) + " returned by authentication service for participant " + (Account.ParticipantId ?? "[null]."));
            }

            if (string.IsNullOrWhiteSpace(AmazonS3Credentials.SecretAccessKey))
            {
                SensusException.Report("Empty " + nameof(AmazonS3Credentials.SecretAccessKey) + " returned by authentication service for participant " + (Account.ParticipantId ?? "[null]."));
            }

            if (string.IsNullOrWhiteSpace(AmazonS3Credentials.SessionToken))
            {
                SensusException.Report("Empty " + nameof(AmazonS3Credentials.SessionToken) + " returned by authentication service for participant " + (Account.ParticipantId ?? "[null]."));
            }

            // save the app state to hang on to the credentials
            await SensusServiceHelper.Get().SaveAsync();

            return AmazonS3Credentials;
        }

        private bool IsExceptionResponse(string json)
        {
            try
            {
                JObject response = JObject.Parse(json);
                return response.ContainsKey("Exception");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ClearCredentials()
        {
            AmazonS3Credentials = null;
        }

        public async Task EnvelopeAsync(byte[] unencryptedBytes, int symmetricKeySizeBits, int symmetricInitializationVectorSizeBits, Stream encryptedOutputStream, CancellationToken cancellationToken)
        {
            try
            {
                if (symmetricKeySizeBits != 256)
                {
                    throw new Exception("Invalid value " + symmetricKeySizeBits + ". Only 256-bit AES is supported.");
                }

                if (symmetricInitializationVectorSizeBits != 128)
                {
                    throw new Exception("Invalid value " + symmetricInitializationVectorSizeBits + ". Only 128-bit IV is supported.");
                }

                AmazonKeyManagementServiceClient kmsClient = new AmazonKeyManagementServiceClient(AmazonS3Credentials.AccessKeyId, AmazonS3Credentials.SecretAccessKey, AmazonS3Credentials.SessionToken);

                kmsClient.ExceptionEvent += (sender, e) =>
                {
                    SensusException.Report("Exception from KMS client:  " + e);
                };

                // generate a symmetric data key
                GenerateDataKeyResponse dataKeyResponse = await kmsClient.GenerateDataKeyAsync(new GenerateDataKeyRequest
                {
                    KeyId = AmazonS3Credentials.CustomerMasterKey,
                    KeySpec = DataKeySpec.AES_256

                }, cancellationToken);

                // write encrypted payload

                // write encrypted data key length and bytes
                byte[] encryptedDataKeyBytes = dataKeyResponse.CiphertextBlob.ToArray();
                byte[] encryptedDataKeyBytesLength = BitConverter.GetBytes(encryptedDataKeyBytes.Length);
                encryptedOutputStream.Write(encryptedDataKeyBytesLength, 0, encryptedDataKeyBytesLength.Length);
                encryptedOutputStream.Write(encryptedDataKeyBytes, 0, encryptedDataKeyBytes.Length);

                // write encrypted random initialization vector length and bytes
                Random random = new Random();
                byte[] initializationVectorBytes = new byte[16];
                random.NextBytes(initializationVectorBytes);

                byte[] encryptedInitializationVectorBytes = (await kmsClient.EncryptAsync(new EncryptRequest
                {
                    KeyId = AmazonS3Credentials.CustomerMasterKey,
                    Plaintext = new MemoryStream(initializationVectorBytes)

                }, cancellationToken)).CiphertextBlob.ToArray();

                byte[] encryptedInitializationVectorBytesLength = BitConverter.GetBytes(encryptedInitializationVectorBytes.Length);
                encryptedOutputStream.Write(encryptedInitializationVectorBytesLength, 0, encryptedInitializationVectorBytesLength.Length);
                encryptedOutputStream.Write(encryptedInitializationVectorBytes, 0, encryptedInitializationVectorBytes.Length);

                // write symmetrically encrypted bytes
                byte[] dataKeyBytes = dataKeyResponse.Plaintext.ToArray();
                SymmetricEncryption symmetricEncryption = new SymmetricEncryption(dataKeyBytes, initializationVectorBytes);
                byte[] encryptedBytes = symmetricEncryption.Encrypt(unencryptedBytes);
                encryptedOutputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            }
            catch (Exception ex)
            {
                SensusException.Report("Exception while running envelope encryption with authentication service:  " + ex.Message, ex);
                throw ex;
            }
        }
    }
}