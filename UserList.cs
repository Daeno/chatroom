using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class UserList
    {
        private static ArrayList userArrayList;


        public UserList()
        {
            userArrayList = new ArrayList();
        }

        public UserList(User[] users)
        {
            try 
            {
                userArrayList = new ArrayList();
                userArrayList.Capacity = users.Length;
                foreach(User u in users) 
                {
                    userArrayList.Add(u);
                }
            }

            catch (Exception ex) {
                Console.Write(ex.ToString());
            }
        }



        public static ArrayList UserArrayList
        {
            get { return userArrayList; }
        }


        

        public static Boolean contains(String account)
        {
            foreach (User u in userArrayList) {
                if (u.Account.Equals(account))
                    return true;
            }

            return false;
        }

        public static Boolean contains(User user)
        {
            return contains(user.Account);
        }


        public static Boolean addUser(User user)
        {
            if (contains(user))
                return false;

            userArrayList.Add(user);

            return true;
        }

        public static Boolean removeUser(User user)
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


        public static User getUserByAcct(String acct)
        {
            foreach (User u in userArrayList) {
                if (u.Account.Equals(acct))
                    return u;
            }

            return null;
        }


        //return "User" ArrayList
        public static ArrayList getOnlineUsers()
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
