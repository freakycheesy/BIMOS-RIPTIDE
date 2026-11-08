namespace BIMOS
{
    public interface IInteractable
    {
        void OnPrimary(bool isButtonDown); //Called when the primary button is pressed
        void OnSecondary(bool isButtonDown); //Called when the secondary button is pressed
        void OnTrigger(bool isButtonDown); //Called when the trigger is pressed
    }
}
