namespace System.Web.UI.WebControls;

public class WizardNavigationEventArgs : EventArgs
{
  private int _currentStepIndex;
  private int _nextStepIndex;
  private bool _cancel;


  public WizardNavigationEventArgs(int currentStepIndex, int nextStepIndex)
  {
    _currentStepIndex = currentStepIndex;
    _nextStepIndex = nextStepIndex;
  }


  public bool Cancel
  {
    get { return _cancel; }
    set { _cancel = value; }
  }


  public int CurrentStepIndex
  {
    get { return _currentStepIndex; }
  }


  public int NextStepIndex
  {
    get { return _nextStepIndex; }
  }

  internal void SetNextStepIndex(int nextStepIndex)
  {
    _nextStepIndex = nextStepIndex;
  }
}