public static class ModRegistry
{
    public static void RegisterAll()
    {
        VariableRegistry.Register(new Variable(
            "playerMoney",
            () =>
            {
                var gi = UnitySingleton<GameInstance>.Instance;
                if (!gi) return 0;

                var player = gi.GetFirstLocalPlayerController();
                if (!player) return 0;

                var emp = player.GetComponent<PlayerControllerEmployment>();
                if (!emp) return 0;

                return emp.GetLocalMoney();
            },
            v =>
            {
                var gi = UnitySingleton<GameInstance>.Instance;
                if (!gi) return;

                var player = gi.GetFirstLocalPlayerController();
                if (!player) return;

                var emp = player.GetComponent<PlayerControllerEmployment>();
                if (!emp) return;

                int current = emp.GetLocalMoney();
                int target = (int)v;

                int delta = target - current;

                if (delta != 0)
                    emp.UpdateMoney(delta);
            },
            typeof(int)
        ));
    
        
        VariableRegistry.Register(new Variable(
            "playerSpaceMoney",
            () =>
            {
                var gi = UnitySingleton<GameInstance>.Instance;
                if (!gi) return 0;

                var player = gi.GetFirstLocalPlayerController();
                if (!player) return 0;

                var emp = player.GetComponent<SpacePlayerControllerEmployment>();
                if (!emp) return 0;

                return emp.GetLocalMoney();
            },
            v =>
            {
                var gi = UnitySingleton<GameInstance>.Instance;
                if (!gi) return;

                var player = gi.GetFirstLocalPlayerController();
                if (!player) return;

                var emp = player.GetComponent<SpacePlayerControllerEmployment>();
                if (!emp) return;

                int current = emp.GetLocalMoney();
                int target = (int)v;

                int delta = target - current;

                if (delta != 0)
                    emp.UpdateMoney(delta);
            },
            typeof(int)
        ));
    }
}