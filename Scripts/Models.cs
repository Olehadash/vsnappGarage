using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserModel
{
    public int id;
    public string name;
    public string user;
    public string password;
    public string email;
    public string login;
    public string mobile;
    public string phone;
    public string fax;
    public string organization;
    public string adres;
    public string city;
    public string passportid;
}

[System.Serializable]
public struct Garage
{
    public int id;
    public string name;
    public string user;
    public string password;
}



[System.Serializable]
public class GaragesModel
{
    public struct GarageModel
    {
        public string name;
        public string telephone;
        public string ofisephone;
        public string insurName;
        public string adress;

    }

    public UserModel[] data;
}

public class Models
{
    public static UserModel user = new UserModel();
    public static GaragesModel garagesmodel = new GaragesModel();

}
