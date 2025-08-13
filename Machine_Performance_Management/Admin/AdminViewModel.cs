using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine_Performance_Management.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Machine_Performance_Management.Admin
{
    public class AdminViewModel : ObservableObject
    {
        protected AdminModel adminModel = new AdminModel();
        #region User_frofile
        protected AdminModel userManagerModel = new AdminModel();
        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "MANAGER", "STAFF" };
        public ObservableCollection<userPasswordModel> _selectedItemDetails { get; set; } = new ObservableCollection<userPasswordModel>();
        public ObservableCollection<userPasswordModel> SelectedItemDetails
        {
            get => _selectedItemDetails;
            set
            {
                _selectedItemDetails = value;
                OnPropertyChanged("StandardInforEditItem");
            }
        }

        public ObservableCollection<UserProfile> userProfiles { get; set; } = new ObservableCollection<UserProfile>();

        public ObservableCollection<UserProfile> UserData
        {
            get => userProfiles;
            set
            {
                userProfiles = value;
                OnPropertyChanged("UserData");
            }
        }
        private UserProfile _selectedUser;
        public UserProfile SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    Username = value.UserName;
                    Password = value.PassWord;
                    Rolename = value.Role;
                    Fullname = value.FullName;
                }
                UpdateSelectedItemDetails();
            }
        }
        private void UpdateSelectedItemDetails()
        {

            if (SelectedUser != null)
            {
                SelectedItemDetails[0].Value = SelectedUser.UserName;
                SelectedItemDetails[1].Value = SelectedUser.PassWord;
                SelectedItemDetails[2].Value = SelectedUser.Role;
                SelectedItemDetails[3].Value = SelectedUser.FullName;

                OnPropertyChanged(nameof(SelectedItemDetails));
            }
        }

        private string username;
        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        private string password;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string rolename;
        public string Rolename
        {
            get => rolename;
            set
            {
                rolename = value;
                OnPropertyChanged("Rolename");
            }
        }

        private string fullname;
        public string Fullname
        {
            get => fullname;
            set
            {
                fullname = value;
                OnPropertyChanged("Fullname");
            }
        }
        #endregion
    }
}
