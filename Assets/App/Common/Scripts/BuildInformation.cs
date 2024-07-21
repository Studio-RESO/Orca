namespace App.Common.Scripts
{
    public struct BuildInformation
    {
        public ClientBuildKey Key
        {
            get;
        }
        
        public string Number
        {
            get;
        }
        
        public string Branch
        {
            get;
        }
        
        public bool IsDevelop
        {
            get
            {
#if DEVELOP
                return true;
#else
                return false;
#endif
            }
        }
        
        public BuildInformation(string number, ClientBuildKey key, string branch)
        {
            Key = key;
            Number = number;
            Branch = branch;
        }
    }
}
