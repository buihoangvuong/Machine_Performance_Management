using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine_Performance_Management.Extension
{
    public enum LoginResult
    {
        Success,           // Đăng nhập thành công
        InvalidUsername,   // Sai tài khoản
        InvalidPassword,   // Sai mật khẩu
        DatabaseError      // Lỗi kết nối database
    }
}
