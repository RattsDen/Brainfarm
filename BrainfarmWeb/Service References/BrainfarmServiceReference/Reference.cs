﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BrainfarmWeb.BrainfarmServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="BrainfarmServiceReference.IBrainfarmService")]
    public interface IBrainfarmService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/GetTimestamp", ReplyAction="http://tempuri.org/IBrainfarmService/GetTimestampResponse")]
        string GetTimestamp();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/GetTimestamp", ReplyAction="http://tempuri.org/IBrainfarmService/GetTimestampResponse")]
        System.Threading.Tasks.Task<string> GetTimestampAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/GetAllUsers", ReplyAction="http://tempuri.org/IBrainfarmService/GetAllUsersResponse")]
        BrainfarmClassLibrary.User[] GetAllUsers();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/GetAllUsers", ReplyAction="http://tempuri.org/IBrainfarmService/GetAllUsersResponse")]
        System.Threading.Tasks.Task<BrainfarmClassLibrary.User[]> GetAllUsersAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/RegisterUser", ReplyAction="http://tempuri.org/IBrainfarmService/RegisterUserResponse")]
        bool RegisterUser(string username, string password, string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/RegisterUser", ReplyAction="http://tempuri.org/IBrainfarmService/RegisterUserResponse")]
        System.Threading.Tasks.Task<bool> RegisterUserAsync(string username, string password, string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/Login", ReplyAction="http://tempuri.org/IBrainfarmService/LoginResponse")]
        string Login(string username, string password, bool keepLoggedIn);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/Login", ReplyAction="http://tempuri.org/IBrainfarmService/LoginResponse")]
        System.Threading.Tasks.Task<string> LoginAsync(string username, string password, bool keepLoggedIn);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/GetCurrentUser", ReplyAction="http://tempuri.org/IBrainfarmService/GetCurrentUserResponse")]
        BrainfarmClassLibrary.User GetCurrentUser(string sessionToken);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/GetCurrentUser", ReplyAction="http://tempuri.org/IBrainfarmService/GetCurrentUserResponse")]
        System.Threading.Tasks.Task<BrainfarmClassLibrary.User> GetCurrentUserAsync(string sessionToken);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/Logout", ReplyAction="http://tempuri.org/IBrainfarmService/LogoutResponse")]
        void Logout(string sessionToken);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IBrainfarmService/Logout", ReplyAction="http://tempuri.org/IBrainfarmService/LogoutResponse")]
        System.Threading.Tasks.Task LogoutAsync(string sessionToken);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IBrainfarmServiceChannel : BrainfarmWeb.BrainfarmServiceReference.IBrainfarmService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class BrainfarmServiceClient : System.ServiceModel.ClientBase<BrainfarmWeb.BrainfarmServiceReference.IBrainfarmService>, BrainfarmWeb.BrainfarmServiceReference.IBrainfarmService {
        
        public BrainfarmServiceClient() {
        }
        
        public BrainfarmServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public BrainfarmServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public BrainfarmServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public BrainfarmServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetTimestamp() {
            return base.Channel.GetTimestamp();
        }
        
        public System.Threading.Tasks.Task<string> GetTimestampAsync() {
            return base.Channel.GetTimestampAsync();
        }
        
        public BrainfarmClassLibrary.User[] GetAllUsers() {
            return base.Channel.GetAllUsers();
        }
        
        public System.Threading.Tasks.Task<BrainfarmClassLibrary.User[]> GetAllUsersAsync() {
            return base.Channel.GetAllUsersAsync();
        }
        
        public bool RegisterUser(string username, string password, string email) {
            return base.Channel.RegisterUser(username, password, email);
        }
        
        public System.Threading.Tasks.Task<bool> RegisterUserAsync(string username, string password, string email) {
            return base.Channel.RegisterUserAsync(username, password, email);
        }
        
        public string Login(string username, string password, bool keepLoggedIn) {
            return base.Channel.Login(username, password, keepLoggedIn);
        }
        
        public System.Threading.Tasks.Task<string> LoginAsync(string username, string password, bool keepLoggedIn) {
            return base.Channel.LoginAsync(username, password, keepLoggedIn);
        }
        
        public BrainfarmClassLibrary.User GetCurrentUser(string sessionToken) {
            return base.Channel.GetCurrentUser(sessionToken);
        }
        
        public System.Threading.Tasks.Task<BrainfarmClassLibrary.User> GetCurrentUserAsync(string sessionToken) {
            return base.Channel.GetCurrentUserAsync(sessionToken);
        }
        
        public void Logout(string sessionToken) {
            base.Channel.Logout(sessionToken);
        }
        
        public System.Threading.Tasks.Task LogoutAsync(string sessionToken) {
            return base.Channel.LogoutAsync(sessionToken);
        }
    }
}
