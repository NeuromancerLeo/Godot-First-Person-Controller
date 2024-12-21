using Godot;
using System;

/// <summary>
/// 玩家的数据管理系统，为全局节点. 用于管理玩家的各项数据及其数值行为，外部（包括 Player）可以通过调用其接口读取或修改各项数值.
/// <para><see cref="PlayerStatisticsManager"/> 承担处理的任务有：将数据限制在规定的大小、根据数据的恢复速度修改其数值、提供数据访问和修改的接口、数据等相关变量的保存与读取</para>
/// <para>注：<see cref="PlayerStatisticsManager"/> 并不处理玩家数据值相关以外的事务，如玩家生命值为 0 时的死亡动画播放、耐力值为 0 时玩家无法跳跃或奔跑的逻辑、玩家使用某种物品、超重逻辑等均不在 <see cref="PlayerStatisticsManager"/> 的职责内.</para>
/// <para><see cref="PlayerStatisticsManager"/> 仅向外提供接口以让外部拥有读取和修改玩家数值的能力，而外部则需依靠这些能力来独自编写对应的游戏逻辑.</para>
/// </summary>
public partial class PlayerStatisticsManager : Node
{
    float _currentVelocity;//当前 Player 的 Velocity. 这个字段是只读的，仅用作 debug 显示用，真正的速度处理是在 Player 的 BasicMovementManager 中进行的，这里只做记录用.
    float _currentInputVelocityAdjustmentPercentage = 1f;//当前 Player 接受的输入速度调整百分比，默认值为 1f. 会用来修改 Player 的输入速度大小，以达到加减速的效果（在外部实现）.

    float _currentHealth;//玩家当前生命值.
    float _currentHealthRecoverySpeed;//当前生命值的恢复速度,单位为值每秒
    const float _maxHealth = 100f;//定义当前生命值的最大值.

    float _currentEndurance;//玩家当前耐力值.
    float _currentEnduranceRecoverySpeed;//当前耐力值的恢复速度.,单位为值每秒
    const float _maxEndurance = 100f;//定义当前耐力值的最大值.

    float _currentHunger;//玩家当前饥饿值.
    float _currentHungerRecoverySpeed;//当前饥饿值的恢复速度.,单位为值每秒
    const float _maxHunger = 100f;//定义当前饥饿值的最大值

    float _currentDefense;//玩家当前防御值.
    const float _maxDefense = 100f;//定义当前防御值的最大值

    float _currentCarryWeight;//当前背包负重,单位为 KG，大小不超过负重上限值.
    float _maxCarryWeight;//背包的最大可携带重量，即负重上限值，单位为 KG.

    /// <summary>
    /// 玩家 Player 当前的瞬时速度.
    /// <para>注：该属性是只读的，仅做 Debug 等显示用.
    /// <para>若想修改玩家的输入速度，请考虑使用 <see cref="InputVelocityAdjustmentPercentage"/></para></para>
    /// </summary>
    public float CurrentVelocity
    {
        get { return _currentVelocity; }
    }

    /// <summary>
    /// 当前玩家 Player 接受输入速度的调整百分比.
    /// <para>注：该属性值参与了 Player 的输入速度计算，故修改该值会导致 Player 的输入速度产生变化</para>
    /// <para>例：</para>
    /// <para>当 <see cref="InputVelocityAdjustmentPercentage"/> = 1f 时，玩家移动速度为默认正常值</para>
    /// <para>当 <see cref="InputVelocityAdjustmentPercentage"/> <![CDATA[>]]> 1f 时，玩家移动速度将会快于默认正常值</para>
    /// <para>当 <see cref="InputVelocityAdjustmentPercentage"/> <![CDATA[<]]> 1f 时，玩家移动速度将会慢于默认正常值</para>
    /// </summary>
    public float InputVelocityAdjustmentPercentage
    {
        get { return _currentInputVelocityAdjustmentPercentage; }
        set { _currentInputVelocityAdjustmentPercentage = value; }
    }

    /// <summary>
    /// 玩家当前的生命值.
    /// <para>注: 赋值会被限制在设定的最大最小值之间</para>
    /// </summary>
    public float Health
    {
        get { return _currentHealth; }
        set { _currentHealth = Math.Clamp(value, 0f, _maxHealth); }
    }

    /// <summary>
    /// 玩家当前生命值的恢复速度，单位为值每秒.
    /// </summary>
    public float HealthRecoverySpeed
    {
        get { return _currentHealthRecoverySpeed; }
        set { _currentHealthRecoverySpeed = value; }
    }

    /// <summary>
    /// 玩家当前的耐力值.
    /// <para>注: 赋值会被限制在设定的最大最小值之间</para>
    /// </summary>
    public float Endurance
    {
        get { return _currentEndurance; }
        set { _currentEndurance = Math.Clamp(value, 0f, _maxEndurance); }
    }

    /// <summary>
    /// 玩家当前耐力值的恢复速度，单位为值每秒.
    /// </summary>
    public float EnduranceRecoverySpeed
    {
        get { return _currentEnduranceRecoverySpeed; }
        set { _currentEnduranceRecoverySpeed = value; }
    }

    /// <summary>
    /// 玩家当前的饥饿值.
    /// <para>注: 赋值会被限制在设定的最大最小值之间</para>
    /// </summary>
    public float Hunger
    {
        get { return _currentHunger; }
        set { _currentHunger = Math.Clamp(value, 0, _maxHunger); }
    }

    /// <summary>
    /// 玩家当前饥饿值的恢复速度，单位为值每秒.
    /// </summary>
    public float HungerRecoverySpeed
    {
        get { return _currentHungerRecoverySpeed; }
        set { _currentHungerRecoverySpeed = value; }
    }

    /// <summary>
    /// 玩家当前的防御值.
    /// <para>注: 赋值会被限制在设定的最大最小值之间</para>
    /// </summary>
    public float Defense
    {
        get { return _currentDefense; }
        set { _currentDefense = Math.Clamp(value, 0, _maxDefense); }
    }

    /// <summary>
    /// 玩家当前的背包负重值.
    /// <para>注: 赋值会被限制在设定的最大最小值之间</para>
    /// </summary>
    public float CarryWeight
    {
        get { return _currentCarryWeight; }
        set { _currentCarryWeight = Math.Clamp(value,0,_maxCarryWeight); }
    }

    //数据初始化
    public override void _Ready()
    {
        //_currentVelocity = ?;
        _currentInputVelocityAdjustmentPercentage = 1f;
        
        _currentHealth = _maxHealth;
        _currentHealthRecoverySpeed = 1f;
        
        _currentEndurance = _maxEndurance;
        _currentEnduranceRecoverySpeed = 5f;
        
        _currentHunger = _maxHunger;
        _currentHungerRecoverySpeed = -1f;
        
        _currentDefense = 0f;

        _maxCarryWeight = 100f;
        _currentCarryWeight = 0f;
        
    }

}
