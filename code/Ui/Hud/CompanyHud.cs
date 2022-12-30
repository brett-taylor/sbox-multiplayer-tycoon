using Sandbox;
using System;

public partial class CompanyHud
{
	public override void Tick()
	{
		base.Tick();

		SetClass( "show-cursor", TycoonHud.Instance.ShowCursor );
	}

	private int GetMoney()
	{
		if ( TycoonGame.TycoonGame.Instance == null || TycoonGame.TycoonGame.Instance.CompanyManager == null )
			return 0;

		return TycoonGame.TycoonGame.Instance.CompanyManager.Money;
	}

	private bool IsCeo()
	{
		return GetCeo() == Game.LocalPawn;
	}

	private bool HasCeo()
	{
		return GetCeo() != null;
	}

	private TycoonGame.Player.Player GetCeo()
	{
		if ( TycoonGame.TycoonGame.Instance == null || TycoonGame.TycoonGame.Instance.CompanyManager == null )
			return null;

		var ceo = TycoonGame.TycoonGame.Instance.CompanyManager.Ceo;
		return ceo;
	}

	private void SetAsCeo()
	{
		ConCmd_SetCeo( Game.LocalPawn.NetworkIdent );
	}

	private void RelinquishCeo()
	{
		ConCmd_RelinquishCeo();
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( GetMoney(), GetCeo() );
	}

	[ConCmd.Server( "set_ceo" )]
	public static void ConCmd_SetCeo( int networkEntity )
	{
		Game.AssertServer();

		var companyController = TycoonGame.TycoonGame.Instance.CompanyManager;
		if ( companyController.Ceo != null )
			return;

		var entityFound = Entity.FindByIndex( networkEntity );

		if ( entityFound != null && entityFound is TycoonGame.Player.Player player )
			companyController.SetCeo( player );
	}

	[ConCmd.Server( "relinquish_ceo" )]
	public static void ConCmd_RelinquishCeo()
	{
		Game.AssertServer();

		var companyController = TycoonGame.TycoonGame.Instance.CompanyManager;
		if ( ConsoleSystem.Caller == companyController.Ceo?.Client )
			companyController.SetCeo( null );
	}
}
