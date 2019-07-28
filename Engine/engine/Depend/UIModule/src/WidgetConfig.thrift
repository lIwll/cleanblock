
enum EUiWidgetType
{
	None			=0;
	Text			=1;
	Image			=2;
	RawImage		=3;
	Button			=4;
	Toggle			=5;
	Slider			=6;
	Scrollbar		=7;
	Dropdown		=8;
	InputField		=9;
	ScrollView		=10;
	Mask			=11;
	ToggleGroup		=12;
	EventData		=13;
	DoTween			=14;
	GridLayoutGroup =15;
	ContentSizeFitter=16;
	GridListEx		=17;
	WidgetGuid		=18;
	UVAnimation		=19;
	FrameAnimation	=20;
	UiGridSingleListEx = 21;
}

struct UiWidgetGuidConfig
{
	1 :optional string			id; 
}

struct UiUVAnimationConfig
{
	1 :optional i32			fps; 
	3 :optional i32			loop; 
	9 :optional i32			w; 
	11:optional i32			h;  
}

struct UiFrameAnimationConfig
{
	1 :optional i32			fps; 
	3 :optional i32			loop;
	5 :optional string		prefix; 
	7 :optional i32			begin_index; 
	9 :optional i32			count; 
	11:optional string		path; 	
}

struct UiColorConfig
{
	1 :optional i32			r; 
	3 :optional i32			g;
	5 :optional i32			b;
	7 :optional i32			a;
}

struct UiRectTransConfig
{
	1 :optional i32			px; 				 
	5 :optional i32			py; 	
	6 :optional i32			pz; 	
	7 :optional i32			delta_size_x; 			
	8 :optional i32			delta_size_y; 		
	9 :optional i32		    anchors_min_x; 		
	11:optional i32		    anchors_max_x; 		
	13:optional i32		 	anchors_min_y;			
	15:optional i32			anchors_max_y; 		
	17:optional i32			pivot_x; 		
	19:optional i32	 		pivot_y; 		
	21:optional i32			rotate_x;
	24:optional i32			rotate_y;
	27:optional i32			rotate_z;
	30:optional i32			scale_x;
	33:optional i32			scale_y;
	36:optional i32			scale_z;
}


struct UiTextConfig
{					  
	1 :optional string		 		text;
	3 :optional string		 		font;
	5 :optional i32			 		font_style;			//EUiFontStyleType	
	7 :optional i32			 		font_size;
	9 :optional i32			 		line_space;	
	11:optional i32			 		rich_text;
	13:optional i32			 		align;
	17:optional i32			 		horizon_overflow;	//0:wrap 	 1:overflow
	19:optional i32			 		vertical_overflow;	//0:truncate 1:overflow
	21:optional i32			 		best_fit;
	23:optional UiColorConfig		color;
	25:optional string		 		material;
	27:optional i32			 		raycast_target;
}

struct UiTransitionConfig
{
	1 :optional i32				interact;
	3 :optional i32				trans_type;
	5 :optional i32				target_graphic;
	7 :optional UiColorConfig	normal_color;
	9 :optional UiColorConfig	light_color;
	11:optional UiColorConfig	press_color;
	13:optional UiColorConfig	disable_color;
	15:optional i32				color_multi;
	17:optional i32				fade_duration;
	19:optional string			light_sprite;
	21:optional string			press_sprite;
	23:optional string			disable_sprite;
}

struct UiImageConfig
{
	1 :optional string			sprite; 
	3 :optional UiColorConfig	color; 
	11:optional string			material;
	13:optional i32				raycast_target;
	15:optional i32				image_type;			//EUiImageType
	17:optional i32				fill_type;			//EUiFillType
	18:optional	i32				fill_method;
	19:optional i32				fill_origin_type;	//EUiFillOriginType
	21:optional i32				fill_amount;		//x1000
	23:optional i32				fill_center;
	24:optional	i32				clock_wise;
	25:optional i32				preserve_aspect;		
}


struct UiRawImageConfig
{					  
	1 :optional string			texture; 
	3 :optional UiColorConfig	color; 
	5 :optional string			material;
	7 :optional i32				raycast_target;	
	9 :optional i32				uv_x;
	11:optional i32				uv_y;
	13:optional i32				uv_w;
	15:optional i32				uv_h;	
}

struct UiButtonConfig
{			
	1 :optional UiTransitionConfig		transition;	
}

struct UiToggleConfig
{					  
	1 :optional UiTransitionConfig		transition;	 
	3 :optional i32						on;
	5 :optional i32						trans_type;
	7 :optional i32						graphic;
	9 :optional i32						group;
}

struct UiToggleGroupConfig
{					  
	1 :optional i32		allow;	 				
}

struct UiEventDataConfig
{					  
	1 :optional list<i32>			type_list;	 
	3 :optional list<string>		lua_list;	
	5 :optional list<string>		method_list;
	7 :optional list<string>		id_list;
}


struct UiSliderConfig
{					  
	1 :optional UiTransitionConfig		transition;	 
	7 :optional i32						fill_rect;
	9 :optional i32						handle_rect;
	11:optional i32						dir;
	13:optional i32	 					min_v;
	15:optional i32	 					max_v;
	17:optional i32	 					whole_num;
	19:optional i32	 					v;
}

struct UiScrollbarConfig
{					  
	1 :optional UiTransitionConfig		transition;	 
	9 :optional i32						handle_rect;
	11:optional i32						dir;
	13:optional i32						value;
	15:optional i32						size;
	17:optional i32						steps;	
}

struct UiDropdownConfig
{					  
	1 :optional UiTransitionConfig		transition;
	11:optional i32						template;
	13:optional i32						cap_text;
	15:optional i32						cap_image;
	17:optional i32						item_text;
	19:optional i32						item_image;
	21:optional i32						value;
	23:optional list<string>			text_list;
	25:optional list<string>			sprite_list;
}

struct UiInputFieldConfig
{			
	1 :optional UiTransitionConfig		transition;
	9 :optional i32		 				text_component; 
	11:optional string		 			text; 
	13:optional i32		 				limit; 	
	15:optional i32		 				content_type; 
	17:optional i32		 				line_type; 
	19:optional i32		 				input_type;
	21:optional i32		 				keyboard_type;
	23:optional i32		 				char_type;
	25:optional i32		 				placeholder;
	27:optional i32		 				blink_rate;
	29:optional UiColorConfig 			select_color;
	31:optional i32		 				hide_mobile_input;
}

struct UiScrollViewConfig
{					  
	1 :optional i32		 content; 
	3 :optional i32		 horizon; 
	5 :optional i32		 vertical; 
	7 :optional i32		 move_type; 
	9 :optional i32		 elasticity; 
	11:optional i32		 inertia; 
	13:optional i32		 dece_rate; 
	15:optional i32		 scroll_sens; 	
	17:optional i32		 view_port; //-2表示Null -1表示引用uiroot >=-1表示引用的对象ID
	19:optional i32		 h_bar; 	//-2表示Null -1表示引用uiroot >=-1表示引用的对象ID
	21:optional i32		 h_see; 
	23:optional i32		 h_space; 
	25:optional i32		 v_bar; 	
	27:optional i32		 v_see; 
	29:optional i32		 v_space; 
}

struct UiMaskConfig
{	
	1 :optional i32		 mask;	
	2 :optional i32		 show_mask;
	3 :optional i32		 mask_active; 
	5 :optional i32		 mask2d;
	7 :optional i32		 mask2d_active;
}

struct UiDoTweenConfig
{					  
	1 :optional i32		 tween_type; 
	2 :optional string	 id; 
	3 :optional i32		 delay; 
	5 :optional i32		 duration; 
	9 :optional i32		 end_x; 
	10:optional i32		 end_y; 
	11:optional i32		 end_z; 
	13:optional i32		 end_float; 
	15:optional UiColorConfig		 end_color;  
	23:optional i32		 end_rect_x; 
	25:optional i32		 end_rect_y; 
	27:optional i32		 end_rect_w; 
	29:optional i32		 end_rect_h; 
	30:optional string	 end_string
	31:optional i32		 auto_kill; 	
	33:optional i32		 auto_play;
	35:optional i32		 ease_type; 	
	37:optional i32		 loop_type; 
	39:optional i32		 loop_count;
	41:optional i32		 ignore_time; 
}

struct UiGridGroupConfig
{					  
	1 :optional i32		pad_left;
	3 :optional i32		pad_right;
	5 :optional i32		pad_top;
	7 :optional i32		pad_bottom;
	9 :optional i32		cell_w;
	11:optional i32		cell_h;
	13:optional i32		space_x;
	15:optional i32		space_y;
	17:optional i32		start_corner;
	19:optional i32		start_axis;
	21:optional i32		alignment;
	23:optional i32		colrow_type;
	25:optional i32		count;
}

struct UiSizeFitterConfig
{					  
	1 :optional i32		h_fit;
	3 :optional i32		v_fit;	
}

struct UiGridListExConfig
{					  
	1 :optional i32		count;	 				
}

struct UiGridSingleListExConfig
{					  
	1 :optional i32		count;	 				
}

struct UiWidgetConfig
{	
	1 :optional i32			 		type;		//EUiWidgetType
	3 :optional i32			 		enable;
	5 :optional UiRectTransConfig	rect;
	7 :optional UiTextConfig		text; 
	9 :optional UiImageConfig		image; 
	11:optional UiRawImageConfig	raw_image; 
	13:optional UiButtonConfig		button; 
	15:optional UiToggleConfig		toggle; 
	17:optional UiToggleGroupConfig	toggle_group;
	19:optional UiSliderConfig		slider; 
	21:optional UiScrollbarConfig	scroll_bar; 
	23:optional UiDropdownConfig	drop_down; 
	25:optional UiInputFieldConfig	input_field; 
	27:optional UiScrollViewConfig	scroll_view;
	29:optional UiMaskConfig		mask;
	31:optional UiEventDataConfig	event_data;
	33:optional list<UiDoTweenConfig>		dotween_list;
	35:optional UiGridGroupConfig	grid_group;
	37:optional UiSizeFitterConfig	size_fitter;
	39:optional UiGridListExConfig	grid_list_ex;
	41:optional	UiWidgetGuidConfig	widget_guid;
	43:optional	UiUVAnimationConfig	uv_animation;
	45:optional	UiFrameAnimationConfig	frame_animation;
	47:optional	UiGridSingleListExConfig	grid_single;
}


struct UiWidgetNodeConfig
{	
	1 :required i32			 			id;
	2 :optional i32			 			active;
	3 :optional i32			 			parent_id;  //-1表示父节点uiroot
	5 :optional string		 			name;
	7 :optional UiRectTransConfig		base_info; //该字段废弃 不使用
	9 :optional list<UiWidgetConfig>	widget_list; 				  			
}

struct UiWidgetConfigTable
{ 
	1 : required list<UiWidgetNodeConfig> Data;
} 


























