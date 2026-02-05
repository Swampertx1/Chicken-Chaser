namespace Characters
{
    public interface IHuman
    {
        public void OnControllerEnabled(HumanController controller);
        public void OnControllerDisabled();
    }
}