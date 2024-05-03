/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

02/05/2024	1.0.0.1		PDO, Skyline	Initial version
****************************************************************************
*/

using System;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.InteractiveAutomationToolkit;
using Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		engine.SetFlag(RunTimeFlags.NoKeyCaching);
		engine.SetFlag(RunTimeFlags.AllowUndef);

		try
		{
			engine.FindInteractiveClient("Script 'Broadcast Message Generator' requires user interaction", 5000);

			RunMainScreen(engine);
		}
		catch (Exception e)
		{
			string message = e.Message;
			engine.GenerateInformation(message);
		}
	}

	private void RunMainScreen(Engine engine)
	{
		InteractiveController controller = new InteractiveController(engine);

		Screen dialog = new Screen(engine);

		dialog.Button.Pressed += (sender, args) => SendMessage(dialog, engine);
		controller.Run(dialog);
	}

	private void SendMessage(Screen dialog, Engine engine)
	{
		MessageDialog messageDialog = new MessageDialog(engine)
		{
			Title = dialog.Title,
		};

		string titleText = dialog.TitleText.Text;
		string messageText = dialog.Message.Text;
		DateTime date = dialog.Date.DateTime;

		if(string.IsNullOrEmpty(titleText) || string.IsNullOrEmpty(messageText))
		{
			messageDialog.Message = "The text fields cannot be empty.";
			messageDialog.Show();
			return;
		}

		if (date < DateTime.Now)
		{
			messageDialog.Message = "The expiration date must be set in the future.";
			messageDialog.Show();
			return;
		}

		BroadcastPopupRequestMessage message = new BroadcastPopupRequestMessage
		{
			PopupInfo = new Skyline.DataMiner.Net.Broadcast.PopupInfo
			{
				Expiration = date,
				Message = messageText,
				Title = titleText,
				Source = Guid.NewGuid(),
			},
		};

		engine.SendSLNetMessage(message);
		engine.ExitSuccess("Broadcast Message Generator - Message Sent.");
	}
}

public class Screen : Dialog
{
	public Screen(Engine engine) : base(engine)
	{
		// Set title
		Title = "Broadcast Message Generator";

		// Init widgets
		TitleText = new TextBox();
		Message = new TextBox();
		Date = new DateTimePicker(DateTime.Now);

		//// Define layout
		AddWidget(new Label("Title: "), 0, 0);
		AddWidget(TitleText, 0, 1);

		AddWidget(new Label("Message: "), 1, 0);
		AddWidget(Message, 1, 1);

		AddWidget(new Label("Expiration Date: "), 2, 0);
		AddWidget(Date, 2, 1);

		AddWidget(Button, 5, 1);
	}

	public Button Button { get; } = new Button("Send")
	{
		Width = 110,
		Height = 30,
	};

	public DateTimePicker Date { get; private set; }

	public TextBox Message { get; private set; }

	public TextBox TitleText { get; private set; }
}