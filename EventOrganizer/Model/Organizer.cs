using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OrganizerService
{
    private User _user;

    public OrganizerService()
    {
        using (ApplicationDBContext db = new ApplicationDBContext())
        {
            _user = new User() { FullName = Environment.UserDomainName, Login = Environment.UserName };

            if (!db.Users.Any(e => e.Login == Environment.UserName)) 
                db.Users.Add(_user);
        }
        Events = new Events(_user);
    }

    public OrganizerService(User user)
    {
        _user = user;
        Init();
        Events = new Events(_user);

    }

    private void Init()
    {
        using (ApplicationDBContext db = new ApplicationDBContext())
        {
            var dbUser = db.Users.FirstOrDefault(e => e.Login == _user.Login);

            if (dbUser != null)
            {
                Events = new Events(dbUser);
            }
            else
            {
                throw new Exception($"Не удалось получить пользователя по login = {_user.Login}");
            }

        }
    }

    public Events Events { get; set; }
}