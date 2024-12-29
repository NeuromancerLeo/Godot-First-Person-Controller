using Godot;
using System;

namespace ZombieWorldWalkDemo.Scripts.Global
{
    /// <summary>
    /// 玩家（Player）的数据管理脚本类，为全局的自动加载脚本。管理玩家（Player）的各项数据及其数值基本行为，外部（包括 Player）可以通过调用其接口读取或修改各项数值.
    /// <para><see cref="PlayerStatisticsManager"/> 承担处理的任务有：作为各种玩家数据的容器、将数据限制在规定的大小、根据数据的恢复速度修改其数值、提供数据访问和修改的接口、对数据进行外部的保存与读取</para>
    /// <para>注：<see cref="PlayerStatisticsManager"/> 并不处理玩家数据值相关以外的事务，如玩家生命值为 0 时的死亡动画播放、耐力值为 0 时玩家无法跳跃或奔跑的逻辑、玩家使用某种物品、超重逻辑等均不在 <see cref="PlayerStatisticsManager"/> 的职责内，
    /// 它仅向外提供接口以让外部拥有读取和修改玩家数值的能力，而外部则需根据这些数值来独自编写对应的游戏逻辑. </para>
    /// <para> <see cref="PlayerStatisticsManager"/> 不包含 Player 的速度（velocity），因为这会涉及到对游戏地图场景中 Player 的引用从而破坏该全局脚本的全局性，若要获取玩家的当前速度值请直接对场景中的 Player 进行引用.</para>
    /// <para><see cref="PlayerStatisticsManager"/> 有以下主要的可用数据：</para>
    /// <see cref="InputVelocityMultiplier"/>, 
    /// <see cref="Health"/>, 
    /// <see cref="Endurance"/>, 
    /// <see cref="Hunger"/>, 
    /// <see cref="Defense"/> 以及
    /// <see cref="CarryWeight"/>.
    /// </summary>
    public partial class PlayerStatisticsManager : Node
    {
        //调用修改数值方法时允许传入的修改类型
        public enum ModifyType
        {
            Add,
            Multiply,
            Set
        }

        /***
         *注：_lastXXXModifier 是用来储存上一次的数值修改者的字符串，在调用 ModifyXXX 方法时需要传入代表数值修改者的 string 做记录方便调试与维修
         */


        //输入速度的倍数
        float _currentInputVelocityMultiplier = 1f;//当前 Player 接受的输入速度倍数，默认值为 1f. Player 会用它来加入输入速度大小的计算，以达到加减速的效果(在外部实现).
        string _lastInputVelocityMultiplierModifier;


        //生命值
        float _currentHealth;//玩家当前生命值.
        string _lastHealthModifier;

        float _currentHealthRecoverySpeed;//当前生命值的恢复速度,单位为值每秒
        string _lastHealthRecoverySpeedModifier;

        const float _maxHealth = 100f;//定义当前生命值的最大值.


        //耐力值
        float _currentEndurance;//玩家当前耐力值.
        string _lastEnduranceModifier;

        float _currentEnduranceRecoverySpeed;//当前耐力值的恢复速度.,单位为值每秒
        string _lastEnduranceRecoverySpeedModifier;

        const float _maxEndurance = 100f;//定义当前耐力值的最大值.


        //饥饿值
        float _currentHunger;//玩家当前饥饿值.
        string _lastHungerModifier;

        float _currentHungerRecoverySpeed;//当前饥饿值的恢复速度.,单位为值每秒
        string _lastHungerRecoverySpeedModifier;

        const float _maxHunger = 100f;//定义当前饥饿值的最大值


        //防御值
        float _currentDefense;//玩家当前防御值.
        string _lastDefenseModifier;

        const float _maxDefense = 100f;//定义当前防御值的最大值


        //背包负重值
        float _currentCarryWeight;//当前背包负重,单位为 KG.
        string _lastCarryWeightModifier;

        float _maxCarryWeight;//定义当前背包的最大可携带重量，即负重上限值，单位为 KG.
        string _lastMaxCarryWeightModifier;


        /*
         * 
         * 下面是外部获取数据值用的只读属性
         * 
         */

        /// <summary>
        /// 当前玩家 Player 接受输入速度的倍数.
        /// <para>只读，若需修改该数值请使用 <see cref="ModifyInputVelocityMultiplier(ModifyType, float, string)"/></para>
        /// </summary>
        public float InputVelocityMultiplier
        {
            get { return _currentInputVelocityMultiplier; }
            private set { _currentInputVelocityMultiplier = Mathf.Clamp(value, 0f, 2f); }
        }

        public string LastInputVelocityMultiplierModifier
        {
            get { return _lastInputVelocityMultiplierModifier; }
            private set { _lastInputVelocityMultiplierModifier = value; }
        }



        /// <summary>
        /// 玩家当前的生命值. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyHealth"/> 方法</para>
        /// </summary>
        public float Health
        {
            get { return _currentHealth; }
            private set { _currentHealth = Math.Clamp(value, 0f, _maxHealth); }
        }

        public string LastHealthModifier
        {
            get { return _lastHealthModifier; }
            private set { _lastHealthModifier = value; }
        }

        /// <summary>
        /// 玩家当前生命值的恢复速度，单位为值每秒. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyHealthRecoverySpeed"/> 方法</para>
        /// </summary>
        public float HealthRecoverySpeed
        {
            get { return _currentHealthRecoverySpeed; }
            private set { _currentHealthRecoverySpeed = value; }
        }

        public string LastHealthRecoverySpeedModifier
        {
            get { return _lastHealthRecoverySpeedModifier; }
            private set { _lastHealthRecoverySpeedModifier = value; }
        }

        /// <summary>
        /// 玩家生命值的设定最大值.
        /// <para>注：该属性是只读的</para>
        /// </summary>
        public float MaxHealth
        {
            get { return _maxHealth; }
        }



        /// <summary>
        /// 玩家当前的耐力值. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyEndurance(ModifyType, float, string)"/> 方法</para>
        /// </summary>
        public float Endurance
        {
            get { return _currentEndurance; }
            private set { _currentEndurance = Math.Clamp(value, 0f, _maxEndurance); }
        }

        public string LastEnduranceModifier
        {
            get { return _lastEnduranceModifier; }
            private set { _lastEnduranceModifier = value; }
        }

        /// <summary>
        /// 玩家当前耐力值的恢复速度，单位为值每秒. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyEnduranceRecoverySpeed(ModifyType, float, string)"/> 方法</para>
        /// </summary>
        public float EnduranceRecoverySpeed
        {
            get { return _currentEnduranceRecoverySpeed; }
            private set { _currentEnduranceRecoverySpeed = value; }
        }

        public string LastEnduranceRecoverySpeedModifier
        {
            get { return _lastEnduranceRecoverySpeedModifier; }
            private set { _lastEnduranceRecoverySpeedModifier = value; }
        }

        /// <summary>
        /// 玩家耐力值的设定最大值.
        /// <para>注：该属性是只读的</para>
        /// </summary>
        public float MaxEndurance
        {
            get { return _maxEndurance; }
        }



        /// <summary>
        /// 玩家当前的饥饿值. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyHunger(ModifyType, float, string)"/> 方法</para>
        /// </summary>
        public float Hunger
        {
            get { return _currentHunger; }
            private set { _currentHunger = Math.Clamp(value, 0, _maxHunger); }
        }

        public string LastHungerModifier
        {
            get { return _lastHungerModifier; }
            private set { _lastHungerModifier = value; }
        }

        /// <summary>
        /// 玩家当前饥饿值的恢复速度，单位为值每秒. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyHungerRecoverySpeed(ModifyType, float, string)"/> 方法</para>
        /// </summary>
        public float HungerRecoverySpeed
        {
            get { return _currentHungerRecoverySpeed; }
            set { _currentHungerRecoverySpeed = value; }
        }

        public string LastHungerRecoverySpeedModifier
        {
            get { return _lastHungerRecoverySpeedModifier; }
            private set { _lastHungerRecoverySpeedModifier = value; }
        }

        /// <summary>
        /// 玩家饥饿值的设定最大值.
        /// <para>注：该属性是只读的</para>
        /// </summary>
        public float MaxHunger
        {
            get { return _maxHunger; }
        }



        /// <summary>
        /// 玩家当前的防御值. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyDefense(ModifyType, float, string)"/> 方法</para>
        /// </summary>
        public float Defense
        {
            get { return _currentDefense; }
            set { _currentDefense = Math.Clamp(value, 0, _maxDefense); }
        }

        public string LastDefenseModifier
        {
            get { return _lastDefenseModifier; }
            private set { _lastDefenseModifier = value; }
        }

        /// <summary>
        /// 玩家防御值的设定最大值.
        /// <para>注：该属性是只读的</para>
        /// </summary>
        public float MaxDefense
        {
            get { return _maxDefense; }
        }



        /// <summary>
        /// 玩家当前的背包负重值. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyCarryWeight(ModifyType, float, string)"/> 方法</para>
        /// </summary>
        public float CarryWeight
        {
            get { return _currentCarryWeight; }
            set { _currentCarryWeight = Math.Clamp(value, 0, _maxCarryWeight); }
        }

        public string LastCarryWeightModifier
        {
            get { return _lastCarryWeightModifier; }
            private set { _lastCarryWeightModifier = value; }
        }

        /// <summary>
        /// 玩家当前的背包最大负重值. 只读
        /// <para>若要修改该数值，请使用 <see cref="ModifyMaxCarryWeight(ModifyType, float, string)"/> 方法</para>
        /// </summary>
        public float MaxCarryWeight
        {
            get { return _maxCarryWeight; }
            set { _maxCarryWeight = Math.Clamp(value, 0, float.PositiveInfinity); }
        }

        public string LastMaxCarryWeightModifier
        {
            get { return _lastMaxCarryWeightModifier; }
            private set { _lastMaxCarryWeightModifier = value; }
        }


        /*
         * 
         * 下面是外部修改各数据值时所要使用的方法，
         * 为什么要专门写方法而不是直接让外部调用属性来修改数据值是因为
         * 我认为有必要记录下数据值的修改者，这样的话就有助于后续的debug.
         * 
         */


        /// <summary>
        /// 可通过该方法修改数据 <see cref="InputVelocityMultiplier"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// <para>注：Player 会使用该属性进行输入速度的计算，故修改该值会导致 Player 的输入速度产生变化</para>
        /// <para>例：</para>
        /// <para>当 <see cref="InputVelocityMultiplier"/> = 1f 时，玩家移动速度为默认正常值</para>
        /// <para>当 <see cref="InputVelocityMultiplier"/> <![CDATA[>]]> 1f 时，玩家移动速度将会快于默认正常值</para>
        /// <para>当 <see cref="InputVelocityMultiplier"/> <![CDATA[<]]> 1f 时，玩家移动速度将会慢于默认正常值</para>
        /// </summary>
        public void ModifyInputVelocityMultiplier(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    InputVelocityMultiplier += value;
                    LastInputVelocityMultiplierModifier = modifier;
                    GD.Print(modifier, " added ", value, " to InputVelocityMultiplier");
                    break;

                case ModifyType.Multiply:
                    InputVelocityMultiplier *= value;
                    LastInputVelocityMultiplierModifier = modifier;
                    GD.Print(modifier, " multiplied InputVelocityMultiplier by", value);
                    break;

                case ModifyType.Set:
                    InputVelocityMultiplier = value;
                    LastInputVelocityMultiplierModifier = modifier;
                    GD.Print(modifier, " set InputVelocityMultiplier to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 InputVelocityMultiplier 时传入了无效的数值修改类型！");
                    break;
            }
        }



        /// <summary>
        /// 可通过该方法修改数据 <see cref="Health"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyHealth(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    Health += value;
                    LastHealthModifier = modifier;
                    GD.Print(modifier, " added ", value, " to Health");
                    break;

                case ModifyType.Multiply:
                    Health *= value;
                    LastHealthModifier = modifier;
                    GD.Print(modifier, " multiplied Health by", value);
                    break;

                case ModifyType.Set:
                    Health = value;
                    LastHealthModifier = modifier;
                    GD.Print(modifier, " set Health to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 Health 时传入了无效的数值修改类型！");
                    break;
            }
        }

        /// <summary>
        /// 可通过该方法修改数据 <see cref="HealthRecoverySpeed"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyHealthRecoverySpeed(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    HealthRecoverySpeed += value;
                    LastHealthRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " added ", value, " to HealthRecoverySpeed");
                    break;

                case ModifyType.Multiply:
                    HealthRecoverySpeed *= value;
                    LastHealthRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " multiplied HealthRecoverySpeed by", value);
                    break;

                case ModifyType.Set:
                    HealthRecoverySpeed = value;
                    LastHealthRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " set HealthRecoverySpeed to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 HealthRecoverySpeed 时传入了无效的数值修改类型！");
                    break;
            }
        }



        /// <summary>
        /// 可通过该方法修改数据 <see cref="Endurance"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyEndurance(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    Endurance += value;
                    LastEnduranceModifier = modifier;
                    GD.Print(modifier, " added ", value, " to Endurance");
                    break;

                case ModifyType.Multiply:
                    Endurance *= value;
                    LastEnduranceModifier = modifier;
                    GD.Print(modifier, " multiplied Endurance by", value);
                    break;

                case ModifyType.Set:
                    Endurance = value;
                    LastEnduranceModifier = modifier;
                    GD.Print(modifier, " set Endurance to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 Endurance 时传入了无效的数值修改类型！");
                    break;
            }
        }

        /// <summary>
        /// 可通过该方法修改数据 <see cref="EnduranceRecoverySpeed"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyEnduranceRecoverySpeed(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    EnduranceRecoverySpeed += value;
                    LastEnduranceRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " added ", value, " to EnduranceRecoverySpeed");
                    break;

                case ModifyType.Multiply:
                    EnduranceRecoverySpeed *= value;
                    LastEnduranceRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " multiplied EnduranceRecoverySpeed by", value);
                    break;

                case ModifyType.Set:
                    EnduranceRecoverySpeed = value;
                    LastEnduranceRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " set EnduranceRecoverySpeed to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 EnduranceRecoverySpeed 时传入了无效的数值修改类型！");
                    break;
            }
        }



        /// <summary>
        /// 可通过该方法修改数据 <see cref="Hunger"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyHunger(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    Hunger += value;
                    LastHungerModifier = modifier;
                    GD.Print(modifier, " added ", value, " to Hunger");
                    break;

                case ModifyType.Multiply:
                    Hunger *= value;
                    LastHungerModifier = modifier;
                    GD.Print(modifier, " multiplied Hunger by", value);
                    break;

                case ModifyType.Set:
                    Hunger = value;
                    LastHungerModifier = modifier;
                    GD.Print(modifier, " set Hunger to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 Hunger 时传入了无效的数值修改类型！");
                    break;
            }
        }

        /// <summary>
        /// 可通过该方法修改数据 <see cref="HungerRecoverySpeed"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyHungerRecoverySpeed(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    HungerRecoverySpeed += value;
                    LastHungerRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " added ", value, " to HungerRecoverySpeed");
                    break;

                case ModifyType.Multiply:
                    HungerRecoverySpeed *= value;
                    LastHungerRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " multiplied HungerRecoverySpeed by", value);
                    break;

                case ModifyType.Set:
                    HungerRecoverySpeed = value;
                    LastHungerRecoverySpeedModifier = modifier;
                    GD.Print(modifier, " set HungerRecoverySpeed to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 HungerRecoverySpeed 时传入了无效的数值修改类型！");
                    break;
            }
        }



        /// <summary>
        /// 可通过该方法修改数据 <see cref="Defense"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyDefense(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    Defense += value;
                    LastDefenseModifier = modifier;
                    GD.Print(modifier, " added ", value, " to Defense");
                    break;

                case ModifyType.Multiply:
                    Defense *= value;
                    LastDefenseModifier = modifier;
                    GD.Print(modifier, " multiplied Defense by", value);
                    break;

                case ModifyType.Set:
                    Defense = value;
                    LastDefenseModifier = modifier;
                    GD.Print(modifier, " set Defense to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 Defense 时传入了无效的数值修改类型！");
                    break;
            }
        }



        /// <summary>
        /// 可通过该方法修改数据 <see cref="CarryWeight"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyCarryWeight(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    CarryWeight += value;
                    LastCarryWeightModifier = modifier;
                    GD.Print(modifier, " added ", value, " to CarryWeight");
                    break;

                case ModifyType.Set:
                    CarryWeight = value;
                    LastCarryWeightModifier = modifier;
                    GD.Print(modifier, " set CarryWeight to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 CarryWeight 时传入了无效的数值修改类型！");
                    break;
            }
        }

        /// <summary>
        /// 可通过该方法修改数据 <see cref="MaxCarryWeight"/> 的值.
        /// <para>第一个参数 <paramref name="modifyType"/> 指定了如何根据传入的 <paramref name="value"/> 来修改数据值：
        /// <see cref="ModifyType.Add"/> 代表相加，<see cref="ModifyType.Multiply"/> 代表相乘，<see cref="ModifyType.Set"/> 代表直接设置值</para>
        /// <para>第三个参数 <paramref name="modifier"/> 代表了对数值进行修改操作的修改者，作为调试纠错的有用信息，调用该方法时应尽可能地清晰明了地指出修改者的身份</para>
        /// </summary>
        public void ModifyMaxCarryWeight(ModifyType modifyType, float value, string modifier)
        {
            switch (modifyType)
            {
                case ModifyType.Add:
                    MaxCarryWeight += value;
                    LastMaxCarryWeightModifier = modifier;
                    GD.Print(modifier, " added ", value, " to MaxCarryWeight");
                    break;

                case ModifyType.Multiply:
                    MaxCarryWeight *= value;
                    LastMaxCarryWeightModifier = modifier;
                    GD.Print(modifier, " multiplied MaxCarryWeight by", value);
                    break;

                case ModifyType.Set:
                    MaxCarryWeight = value;
                    LastMaxCarryWeightModifier = modifier;
                    GD.Print(modifier, " set MaxCarryWeight to ", value);
                    break;

                default:
                    GD.Print(modifier, " 在修改 MaxCarryWeight 时传入了无效的数值修改类型！");
                    break;
            }
        }







        //数据初始化
        public override void _Ready()
        {
            _currentInputVelocityMultiplier = 1f;

            _currentHealth = _maxHealth;
            _currentHealthRecoverySpeed = 0.05f;

            _currentEndurance = _maxEndurance;
            _currentEnduranceRecoverySpeed = 1f;

            _currentHunger = _maxHunger;
            _currentHungerRecoverySpeed = -0.05f;

            _currentDefense = 0f;

            _maxCarryWeight = 100f;
            _currentCarryWeight = 0f;

        }

        public override void _PhysicsProcess(double delta)
        {
            //根据恢复速度每帧更新数据值
            _currentHealth = Math.Clamp((_currentHealth + _currentHealthRecoverySpeed * (float)delta), 0, _maxHealth);
            _currentEndurance = Math.Clamp((_currentEndurance + _currentEnduranceRecoverySpeed * (float)delta), 0, _maxEndurance);
            _currentHunger = Math.Clamp((_currentHunger + _currentHungerRecoverySpeed * (float)delta), 0, _maxHunger);
        }

    }
}
