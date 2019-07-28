include "TV3.thrift"

struct EffectInfoConfig
{
	1 :optional string					path; 		//路径
	3 :optional TV3.ThriftVector3		offset; 	//偏移量
	5 :optional TV3.ThriftVector3		scale; 		//缩放
	7 :optional TV3.ThriftVector3		rotation; 	//旋转
	9 :optional i32						mount_point;//挂载点
	11:optional i32						duration;	//持续时间
}

struct TrailInfoConfig
{
	1 :optional i32							type; 			//0:直线 1:电链 2:流星 3:导弹 4:螺旋 5:抛射
	3 :optional i32							start_speed;	//初始速度
	5 :optional i32							acc_spped;		//加速度
	7 :optional i32							z_angle;		//Z轴偏移角度
	9 :optional i32							high;			//发射高度
	11:optional i32							radom_radii;	//随机半径
	13:optional i32							radom_distance;	//随机飞行距离
	15:optional i32							dur_time;		//闪电链时间
	17:optional i32							rotate_speed;	//螺旋速度
	19:optional i32							rotate_radii; 	//螺旋半径
	21:optional i32							cast_high; 		//抛射高度
}

struct FlySkillConfig
{
	1 :optional i32								id; 
	3 :optional i32								time; 
	5 :optional i32								delay;			//延迟播放时间
	7 :EffectInfoConfig							effect_start;	//开始特效
	9 :EffectInfoConfig							effect_fly;		//飞行特效
	11:EffectInfoConfig							effect_hit;		//击中特效
	13:EffectInfoConfig							effect_damage;	//伤害特效
	15:optional i32								target_type;	//0:pos list  1:obj list
	17:optional TrailInfoConfig					trail			//飞行轨迹
	19:optional i32								limit_type; 	//0:无限制 1:时间限制 2:距离限制
	21:optional i32								limit_duration;	//最大飞行时间	
	23:optional i32								limit_distance;	//最大飞行距离
    25:optional string                          AudioClipPath;//音频地址（自主添加的，非自动生成）
	}

struct FlySkillConfigTable
{ 
	1 : required list<FlySkillConfig> Data;
} 


























