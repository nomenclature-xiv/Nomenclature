using System;
using System.Timers;

namespace NomenclatureClient.UI;

public class PrimaryWindowController
{
    private readonly Timer _waitForRegistrationTimer = new(10 * 1000);

    // The time at which the registration will start
    private DateTime _waitForRegistrationEndTime; 
    //
    public PrimaryWindowController()
    {
        _waitForRegistrationTimer.Enabled = false;
        _waitForRegistrationTimer.AutoReset = false;
        _waitForRegistrationTimer.Elapsed += OnWaitForRegistration;
    }

    /// <summary>
    ///     Which step 
    /// </summary>
    public uint Step { get; private set; }
    
    /// <summary>
    ///     Check if we are still waiting
    /// </summary>
    public int WaitingForRegistrationTimeRemaining => (int)(_waitForRegistrationEndTime - DateTime.Now).TotalSeconds;

    /// <summary>
    ///     Open the registration window
    /// </summary>
    public void OpenXivAuthRegistrationWindow()
    {
        // Do something
        Step = 1;
    }

    /// <summary>
    ///     Confirm that our character was registered
    /// </summary>
    public void ConfirmRegistration()
    {
        // Do something
        _waitForRegistrationEndTime = DateTime.Now.AddSeconds(10);
        _waitForRegistrationTimer.Stop();
        _waitForRegistrationTimer.Start();
    }
    
    private void OnWaitForRegistration(object? sender, ElapsedEventArgs e)
    {
        // Do something
        Step = 2;
    }
}