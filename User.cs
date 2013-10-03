using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class User
    {
        String account;
        String password;

        //string array
        ArrayList friendList;
        ArrayList blackList;

        bool isOnline;



        public User()
        {
            isOnline = false;

            setFriendsList(null);
            setBlackList(null);
        }

        public User(String acct, String pw)
        {   
            account  = acct;
            password = pw;
            isOnline = false;

            setFriendsList(null);
            setBlackList(null);
        }

        public User(String acct, String pw, String[] friendsArr, String[] blackListArr)
        {
            account = acct;
            password = pw;
            isOnline = false;

            setFriendsList(friendsArr);
            setBlackList(blackListArr);
        }


        public bool IsOnline
        {
            set { isOnline = value; }
            get { return isOnline; }
        }

        public String Account
        {
            get { return account; }
        }



        public void changePassword(String pw)
        {
            password = pw;
        }




        public bool addFriend(String acct)
        {
            if (friendList.Contains(acct))
                return false;
            else
                friendList.Add(acct);

            return true;
        }

        public bool removeFriend(String acct)
        {
            if (!friendList.Contains(acct))
                return false;
            else {
                friendList.Remove(acct);
                removeBlack(acct);
            }

            return true;
        }

        public bool addBlack(String acct)
        {
            if (blackList.Contains(acct))
                return false;
            else
                blackList.Add(acct);

            return true;
        }

        public bool removeBlack(String acct)
        {
            if (!blackList.Contains(acct))
                return false;
            else
                blackList.Remove(acct);

            return true;
        }




        private void setFriendsList(String[] friendsArr)
        {
            setList(friendList, friendsArr);
        }

        private void setBlackList(String[] blackListArr)
        {
            setList(blackList, blackListArr);
        }


        private void setList(ArrayList arrList, String[] arr)
        {
            if (arr == null) {
                arrList = new ArrayList();
                return;
            }

            else {
                arrList.Capacity = arr.Length;

                foreach (String s in arr)
                    arrList.Add(s);
            }
        }


        public ArrayList getOnLineFriendList()
        {
            ArrayList onLineFriendList = new ArrayList();

            foreach (String acct in friendList) {
                User friend = UserList.getUserByAcct(acct);
                
                if (friend.isOnline) {
                    onLineFriendList.Add(acct);
                }
            }

            return onLineFriendList;
        }


        public Boolean checkPassword(String pw)
        {
            return (password.Equals(pw));
        }



    }

}
