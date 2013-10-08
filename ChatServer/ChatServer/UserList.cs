using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class UserList
    {
        private ArrayList userArrayList;


        public UserList()
        {
            userArrayList = new ArrayList();
        }

        public UserList(User[] users)
        {
            try {
                userArrayList = new ArrayList();
                userArrayList.Capacity = users.Length;
                foreach(User u in users) {
                    userArrayList.Add(u);
                }
            }

            catch (Exception ex) {
                Console.Write(ex.ToString());
            }
        }



        public ArrayList UserArrayList
        {
            get { return userArrayList; }
        }


        

        public Boolean contains(String account)
        {
            foreach (User u in userArrayList) {
                if (u.Account.Equals(account))
                    return true;
            }

            return false;
        }

        public Boolean contains(User user)
        {
            return contains(user.Account);
        }


        public Boolean addUser(User user)
        {
            if (contains(user))
                return false;

            userArrayList.Add(user);

            return true;
        }

        public Boolean removeUser(User user)
        {
            if (!contains(user))
                return false;

            foreach (User u in userArrayList) {
                if (u.Account.Equals(user.Account)) {
                    userArrayList.Remove(u);
                }
                else {
                    u.removeFriend(u.Account);
                }
            }

            return false;
        }


        public User getUserByAcct(String acct)
        {
            foreach (User u in userArrayList) {
                if (u.Account.Equals(acct))
                    return u;
            }

            return null;
        }


        //return "User" ArrayList
        public ArrayList getOnlineUsers()
        {
            ArrayList onlineUsers = new ArrayList();

            foreach (User u in userArrayList) {
                if (u.IsOnline)
                    onlineUsers.Add(u);
            }

            return onlineUsers;
        }



    }
}
