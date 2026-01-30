using System.Security.Cryptography.X509Certificates;
using Godot;
using hd2dtest.Scripts.Core;

public partial class HouseAreaHandler : CsgCombiner3D
{
	public CsgPolygon3D InHouseNode { get; set; }
	public Area3D HouseArea3D { get; set;}
	public CsgPolygon3D HouseNode { get; set; }
	public CsgBox3D box { get; set; }
	public CsgBox3D door { get; set; }

	public override void _Ready()
	{
		HouseNode = GetNode<CsgPolygon3D>("House");
		InHouseNode = GetNode<CsgPolygon3D>("inHouse");
		HouseArea3D = GetNode<Area3D>("House_Area3D");
		box = GetNode<CsgBox3D>("box");
		door = GetNode<CsgBox3D>("door");
		// 配置房间大小
		// int center = HouseNode.Polygon;
		// Log.Debug($"center: {center}");
		// for (int i = 0; i < HouseNode.Polygon.Count; i++)
		// {
		// 	var p = HouseNode.Polygon[i];
		// 	// 中间左边的x+0.1，右边的x-0.1，正中间的y*2
		// 	if (i < center)
		// 		p.X += 0.1f;
		// 	else if (i > center)
		// 		p.X -= 0.1f;
		// 	if (i == center)
		// 		p.Y *= 2;
		// 	InHouseNode.Polygon[i] = p;
		// }

		HouseArea3D.BodyEntered += OnHouseArea3DBodyEntered;
		HouseArea3D.BodyExited += OnHouseArea3DBodyExited;
	}

	public void OnHouseArea3DBodyEntered(Node body)
	{
		// 检查进入的是否是玩家角色
		if (body.Name == "Player" || body.IsInGroup("player"))
		{
			if (InHouseNode != null)
			{
				InHouseNode.Visible = true;
				Log.Info("Player entered house area, showing inHouse");
			}
		}
	}

	public void OnHouseArea3DBodyExited(Node3D body)
	{
		// 检查离开的是否是玩家角色
		if (body.Name == "Player" || body.IsInGroup("player"))
		{
			if (InHouseNode != null)
			{
				InHouseNode.Visible = false;
				Log.Info("Player exited house area, hiding inHouse");
			}
		}
	}
}
