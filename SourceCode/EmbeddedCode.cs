﻿namespace CScript {
    public class Embedded {
        protected static string BuiltInFunctions =
"""
// BUILT IN FUNCTIONS
delegate void _print(string str);
_print print = null;

delegate void _assert(bool condition, string message);
_assert assert = null;
""";
        protected static string ArrayAPI =
"""
// ARRAY API
delegate num    _array_length(object array);
delegate num    _array_first(object array, object value);
delegate num    _array_last(object array, object value);
delegate object _array_at(object array, num index);
delegate object _array_concat(object array, object otherArray); 
delegate string _array_join(object array, string delim);
delegate object _array_pop(object array);
delegate object _array_shift(object array);
delegate object _array_slice(object array, num start, num count);
delegate void   _array_sort(object array, _array_comparer comparer);
delegate void   _array_reverse(object array);
delegate void   _array_delete(object array, num start, num count);
delegate void   _array_add(object array, object item);
delegate void   _array_insert(object array, num index, object item);
delegate object _array_copy(object array);
delegate void   _array_clear(object array);
delegate num    _array_comparer(object left, object right);

struct _array {
    _array_length       length;   
    _array_first        first;    
    _array_last         last;     
    _array_at           at;       
    _array_concat       concat;   
    _array_join         join;     
    _array_pop          pop;      
    _array_shift        shift;    
    _array_slice        slice;    
    _array_sort         sort;
    _array_reverse      reverse;  
    _array_delete       remove;   
    _array_add          add;    
    _array_insert       insert;
    _array_copy         copy;     
    _array_clear        clear;    
}

_array array = new _array();
""";
        protected static string MapAPI =
"""
// MAP API
delegate void _map_clear(object map);
delegate object _map_delete(object map, object key);
delegate object _map_get(object map, object key);
delegate void _map_set(object map, object key, object value);
delegate bool _map_has(object map, object key);
delegate object _map_keys(object map);
delegate object _map_values(object map);

struct _map {
    _map_clear  clear;
    _map_delete remove;
    _map_get    get;
    _map_set    set;
    _map_has    has;
    _map_keys   keys;
    _map_values values;
}

_map map = new _map(); 
""";
        protected static string StringAPI =
"""
// STRING API
delegate void _str_len(string str);
delegate char _str_at(string str, num index);
delegate string _str_concat(string left, string right);
delegate bool _str_ends(string str, string endsWith);
delegate bool _str_starts(string str, string startsWith);
delegate num _str_first(string str, string search);
delegate num _str_last(string str, string search);
delegate string _str_replace(string str, string replaceWhat, string replaceWith);
delegate string[] _str_split(string str, string delim);
delegate string _str_substr(string str, num start, num count);
delegate string _str_lower(string str);
delegate string _str_upper(string str);

struct _string {
    _str_len length;        
    _str_at at;             
    _str_concat concat;     
    _str_ends ends;         
    _str_starts starts;     
    _str_first first;       
    _str_last last;         
    _str_split split;       
    _str_lower lower;       
    _str_upper upper;       
    _str_replace replace;
    _str_substr substring;

}

_string string = new _string();
""";
        protected static string FileAPI =
"""
delegate void OnFileLoaded(string path, char[] data);
delegate void OnFileOperation(string path, bool success);

delegate void _load_file(string path, OnFileLoaded callback);
delegate void _write_file(string path, OnFileOperation callback);
delegate void _fs_op(string path, OnFileOperation callback);

struct _file {
    _load_file read;
    _write_file write;
    _fs_op mkdir;
    _fs_op rmdir;
    _fs_op rmdoc;
    _fs_op isdir;
    _fs_op isdoc;    
    _fs_op exists;
    bool available = false;
}

_file file = new _file();
""";
        protected static string MathAPI =
"""
delegate num _math_no_arg();
delegate num _math_one_arg(num n);
delegate num _math_two_arg(num a, num b);

struct _math {
    num PI;

    _math_one_arg abs;
    _math_one_arg cos;
    _math_one_arg sin;
    _math_one_arg tan;
    _math_one_arg sqrt;
    _math_one_arg exp;
    _math_one_arg pow;
    _math_no_arg random;
    _math_one_arg acos;
    _math_one_arg asin;    
    _math_one_arg atan;
    _math_two_arg atan2;
    _math_two_arg imul;
    _math_two_arg max;
    _math_two_arg min;
    _math_two_arg log;
    _math_one_arg ceil;    
    _math_one_arg floor;
    _math_one_arg round;
    _math_one_arg sign;
    _math_one_arg trunc;
}

_math Math = new _math();
_math math = Math;
""";
        protected static string VectorAPI =
"""
struct vec2 {
    num x = 0.0;
    num y = 0.0;
}

struct vec3 {
    num x = 0.0;
    num y = 0.0;
    num z = 0.0;
}

struct vec4 {
    num x = 0.0;
    num y = 0.0;
    num z = 0.0;
    num w = 0.0;
}

struct _vec2 {
    num epsilon = 0.0001;
    _vec2_bin           add         = _vec2_add;
    _vec2_bin           sub         = _vec2_sub;
    _vec2_bin           mul         = _vec2_mul;
    _vec2_bin           div         = _vec2_div;
    _vec2_flt           scale       = _vec2_scale;
    _vec2_bin_num       dot         = _vec2_dot;
    _vec2_num           len         = _vec2_len;
    _vec2_num           lenSq       = _vec2_lenSq;
    _vec2_vec2          normalized  = _vec2_normalized;
    _vec2_bin_num       angle       = _vec2_angle;
    _vec2_bin           project     = _vec2_project;
    _vec2_bin           reject      = _vec2_reject;
    _vec2_bin           reflect     = _vec2_reflect;
    _vec2_interpolate   lerp        = _vec2_lerp;
    _vec2_interpolate   slerp       = _vec2_slerp;
    _vec2_interpolate   nlerp       = _vec2_nlerp;
    _vec2_bin_bool      compare     = _vec2_compare;
}

struct _vec3 {
    num epsilon = 0.0001;
    _vec3_bin           add         = _vec3_add;
    _vec3_bin           sub         = _vec3_sub;
    _vec3_bin           mul         = _vec3_mul;
    _vec3_bin           div         = _vec3_div;
    _vec3_flt           scale       = _vec3_scale;
    _vec3_bin_num       dot         = _vec3_dot;
    _vec3_num           len         = _vec3_len;
    _vec3_num           lenSq       = _vec3_lenSq;
    _vec3_vec3          normalized  = _vec3_normalized;
    _vec3_bin_num       angle       = _vec3_angle;
    _vec3_bin           project     = _vec3_project;
    _vec3_bin           reject      = _vec3_reject;
    _vec3_bin           reflect     = _vec3_reflect;
    _vec3_bin           cross       = _vec3_cross;
    _vec3_interpolate   lerp        = _vec3_lerp;
    _vec3_interpolate   slerp       = _vec3_slerp;
    _vec3_interpolate   nlerp       = _vec3_nlerp;
    _vec3_bin_bool      compare     = _vec3_compare;
}

struct _vec4 {
    num epsilon = 0.0001;
    _vec4_bin           add         = _vec4_add;
    _vec4_bin           sub         = _vec4_sub;
    _vec4_bin           mul         = _vec4_mul;
    _vec4_bin           div         = _vec4_div;
    _vec4_flt           scale       = _vec4_scale;
    _vec4_bin_num       dot         = _vec4_dot;
    _vec4_num           len         = _vec4_len;
    _vec4_num           lenSq       = _vec4_lenSq;
    _vec4_vec4          normalized  = _vec4_normalized;
    _vec4_bin_num       angle       = _vec4_angle;
    _vec4_bin           project     = _vec4_project;
    _vec4_bin           reject      = _vec4_reject;
    _vec4_bin           reflect     = _vec4_reflect;
    _vec4_bin_vec3      cross       = _vec4_cross;
    _vec4_interpolate   lerp        = _vec4_lerp;
    _vec4_interpolate   slerp       = _vec4_slerp;
    _vec4_interpolate   nlerp       = _vec4_nlerp;
    _vec4_bin_bool      compare     = _vec4_compare;
}

vec2 _vec2_add(vec2 left, vec2 right) {
    return new vec2(
        left.x + right.x, 
        left.y + right.y 
    );
}

vec3 _vec3_add(vec3 left, vec3 right) {
    return new vec3(
        left.x + right.x, 
        left.y + right.y, 
        left.z + right.z
    );
}

vec4 _vec4_add(vec4 left, vec4 right) {
    return new vec4(
        left.x + right.x, 
        left.y + right.y, 
        left.z + right.z,
        left.w + right.w
    );
}

vec2 _vec2_sub(vec2 left, vec2 right) {
    return new vec2(
        left.x - right.x, 
        left.y - right.y 
    );
}

vec3 _vec3_sub(vec3 left, vec3 right) {
    return new vec3(
        left.x - right.x, 
        left.y - right.y, 
        left.z - right.z
    );
}

vec4 _vec4_sub(vec4 left, vec4 right) {
    return new vec4(
        left.x - right.x, 
        left.y - right.y, 
        left.z - right.z,
        left.w - right.w
    );
}

vec2 _vec2_mul(vec2 left, vec2 right) {
    return new vec2(
        left.x * right.x, 
        left.y * right.y 
    );
}

vec3 _vec3_mul(vec3 left, vec3 right) {
    return new vec3(
        left.x * right.x, 
        left.y * right.y, 
        left.z * right.z
    );
}

vec4 _vec4_mul(vec4 left, vec4 right) {
    return new vec4(
        left.x * right.x, 
        left.y * right.y, 
        left.z * right.z,
        left.w * right.w
    );
}

vec2 _vec2_div(vec2 left, vec2 right) {
    return new vec2(
        left.x / right.x, 
        left.y / right.y 
    );
}

vec3 _vec3_div(vec3 left, vec3 right) {
    return new vec3(
        left.x / right.x, 
        left.y / right.y, 
        left.z / right.z
    );
}

vec4 _vec4_div(vec4 left, vec4 right) {
    return new vec4(
        left.x / right.x, 
        left.y / right.y, 
        left.z / right.z,
        left.w / right.w
    );
}

vec2 _vec2_scale(vec2 left, num right) {
    return new vec2(
        left.x * right, 
        left.y * right 
    );
}

vec3 _vec3_scale(vec3 left, num right) {
    return new vec3(
        left.x * right, 
        left.y * right, 
        left.z * right
    );
}

vec4 _vec4_scale(vec4 left, num right) {
    return new vec4(
        left.x * right, 
        left.y * right, 
        left.z * right,
        left.w * right
    );
}

num _vec2_dot(vec2 left, vec2 right) {
    return left.x * right.x + left.y * right.y;
}

num _vec3_dot(vec3 left, vec3 right) {
    return left.x * right.x + left.y * right.y + left.z * right.z;
}

num _vec4_dot(vec4 left, vec4 right) {
    return left.x * right.x + left.y * right.y + left.z * right.z + left.w * right.w;
}

num _vec2_lenSq(vec2 v) {
    return v.x * v.x + v.y * v.y;
}

num _vec3_lenSq(vec3 v) {
    return v.x * v.x + v.y * v.y + v.z * v.z;
}

num _vec4_lenSq(vec4 v) {
    return v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
}

num _vec2_len(vec2 v) {
    num res = v.x * v.x + v.y * v.y;
    if (res < vec2.epsilon) {
        return 0.0;
    }
    return Math.sqrt(res);
}

num _vec3_len(vec3 v) {
    num res = v.x * v.x + v.y * v.y + v.z * v.z;
    if (res < vec3.epsilon) {
        return 0.0;
    }
    return Math.sqrt(res);
}

num _vec4_len(vec4 v) {
    num res = v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
    if (res < vec4.epsilon) {
        return 0.0;
    }
    return Math.sqrt(res);
}

vec2 _vec2_normalized(vec2 v) {
    num res = v.x * v.x + v.y * v.y;
    if (res < vec2.epsilon) {
        return v;
    }
    num invLen = 1.0 / math.sqrt(res);

    return new vec2(v.x * invLen, v.y * invLen);
}

vec3 _vec3_normalized(vec3 v) {
    num res = v.x * v.x + v.y * v.y + v.z * v.z;
    if (res < vec3.epsilon) {
        return v;
    }
    num invLen = 1.0 / math.sqrt(res);

    return new vec3(v.x * invLen, v.y * invLen, v.z * invLen);
}

vec4 _vec4_normalized(vec4 v) {
    num res = v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
    if (res < vec4.epsilon) {
        return v;
    }
    num invLen = 1.0 / math.sqrt(res);

    return new vec4(v.x * invLen, v.y * invLen, v.z * invLen, v.w * invLen);
}

num _vec2_angle(vec2 l, vec2 r) {
	num sqMagL = l.x * l.x + l.y * l.y;
	num sqMagR = r.x * r.x + r.y * r.y;

	if (sqMagL < vec2.epsilon or sqMagR < vec2.epsilon) {
		return 0.0;
	}

	num dot = l.x * r.x + l.y * r.y;
	num len = math.sqrt(sqMagL) * math.sqrt(sqMagR);
	return math.acos(dot / len);
}

num _vec3_angle(vec3 l, vec3 r) {
	num sqMagL = l.x * l.x + l.y * l.y + l.z * l.z;
	num sqMagR = r.x * r.x + r.y * r.y + r.z * r.z;

	if (sqMagL < vec3.epsilon or sqMagR < vec3.epsilon) {
		return 0.0;
	}

	num dot = l.x * r.x + l.y * r.y + l.z * r.z;
	num len = math.sqrt(sqMagL) * math.sqrt(sqMagR);
	return math.acos(dot / len);
}

num _vec4_angle(vec4 l, vec4 r) {
	num sqMagL = l.x * l.x + l.y * l.y + l.z * l.z + l.w * l.w;
	num sqMagR = r.x * r.x + r.y * r.y + r.z * r.z + r.w * r.w;

	if (sqMagL < vec4.epsilon or sqMagR < vec4.epsilon) {
		return 0.0;
	}

	num dot = l.x * r.x + l.y * r.y + l.z * r.z * l.w * r.w;
	num len = math.sqrt(sqMagL) * math.sqrt(sqMagR);
	return math.acos(dot / len);
}

vec2 _vec2_project(vec2 a, vec2 b) {
	num magBSq = _vec2_len(b);
	if (magBSq < vec2.epsilon) {
		return new vec2();
	}
	num scale = _vec2_dot(a, b) / magBSq;
	return b * scale;
}

vec3 _vec3_project(vec3 a, vec3 b) {
	num magBSq = _vec3_len(b);
	if (magBSq < vec3.epsilon) {
		return new vec3();
	}
	num scale = _vec3_dot(a, b) / magBSq;
	return b * scale;
}

vec4 _vec4_project(vec4 a, vec4 b) {
	num magBSq = _vec4_len(b);
	if (magBSq < vec4.epsilon) {
		return new vec4();
	}
	num scale = _vec4_dot(a, b) / magBSq;
	return b * scale;
}

vec2 _vec2_reject(vec2 a, vec2 b) {
	vec2 projection = _vec2_project(a, b);
	return a - projection;
}

vec3 _vec3_reject(vec3 a, vec3 b) {
	vec3 projection = _vec3_project(a, b);
	return a - projection;
}

vec4 _vec4_reject(vec4 a, vec4 b) {
	vec4 projection = _vec4_project(a, b);
	return a - projection;
}

vec2 _vec2_reflect(vec2 a, vec2 b) {
	num magBSq = _vec2_len(b);
	if (magBSq < vec2.epsilon) {
		return new vec2();
	}
	num scale = _vec2_dot(a, b) / magBSq;
	vec2 proj2 = b * (scale * 2);
	return a - proj2;
}

vec3 _vec3_reflect(vec3 a, vec3 b) {
	num magBSq = _vec3_len(b);
	if (magBSq < vec3.epsilon) {
		return new vec3();
	}
	num scale = _vec3_dot(a, b) / magBSq;
	vec3 proj2 = b * (scale * 2);
	return a - proj2;
}

vec4 _vec4_reflect(vec4 a, vec4 b) {
	num magBSq = _vec4_len(b);
	if (magBSq < vec4.epsilon) {
		return new vec4();
	}
	num scale = _vec4_dot(a, b) / magBSq;
	vec4 proj2 = b * (scale * 2);
	return a - proj2;
}

vec3 _vec3_cross(vec3 l, vec3 r) {
	return new vec3(
		l.y * r.z - l.z * r.y,
		l.z * r.x - l.x * r.z,
		l.x * r.y - l.y * r.x
	);
}

vec3 _vec4_cross(vec4 l, vec4 r) {
	return new vec3(
		l.y * r.z - l.z * r.y,
		l.z * r.x - l.x * r.z,
		l.x * r.y - l.y * r.x
	);
}

vec2 _vec2_lerp(vec2 s, vec2 e, num t) {
	return new vec2(
		s.x + (e.x - s.x) * t,
		s.y + (e.y - s.y) * t
	);
}

vec3 _vec3_lerp(vec3 s, vec3 e, num t) {
	return new vec3(
		s.x + (e.x - s.x) * t,
		s.y + (e.y - s.y) * t,
		s.z + (e.z - s.z) * t
	);
}

vec4 _vec4_lerp(vec4 s, vec4 e, num t) {
	return new vec4(
		s.x + (e.x - s.x) * t,
		s.y + (e.y - s.y) * t,
		s.z + (e.z - s.z) * t,
        s.w + (e.w - s.w) * t
	);
}

vec2 _vec2_slerp(vec2 s, vec2 e, num t) {
	if (t < vec2.epsilon) {
		return _vec2_lerp(s, e, t);
	}

	vec2 from = _vec2_normalized(s);
	vec2 to = _vec2_normalized(e);

	num theta = _vec2_angle(from, to);
	num sin_theta = math.sin(theta);

	num a = math.sin((1.0 - t) * theta) / sin_theta;
	num b = math.sin(t * theta) / sin_theta;

	return from * a + to * b;
}

vec3 _vec3_slerp(vec3 s, vec3 e, num t) {
	if (t < vec3.epsilon) {
		return _vec3_lerp(s, e, t);
	}

	vec3 from = _vec3_normalized(s);
	vec3 to = _vec3_normalized(e);

	num theta = _vec3_angle(from, to);
	num sin_theta = math.sin(theta);

	num a = math.sin((1.0 - t) * theta) / sin_theta;
	num b = math.sin(t * theta) / sin_theta;

	return from * a + to * b;
}

vec4 _vec4_slerp(vec4 s, vec4 e, num t) {
	if (t < vec4.epsilon) {
		return _vec4_lerp(s, e, t);
	}

	vec4 from = _vec4_normalized(s);
	vec4 to = _vec4_normalized(e);

	num theta = _vec4_angle(from, to);
	num sin_theta = math.sin(theta);

	num a = math.sin((1.0 - t) * theta) / sin_theta;
	num b = math.sin(t * theta) / sin_theta;

	return from * a + to * b;
}

vec2 _vec2_nlerp(vec2 s, vec2 e, num t) {
	vec2 linear = new vec2(
		s.x + (e.x - s.x) * t,
		s.y + (e.y - s.y) * t
	);
	return _vec2_normalized(linear);
}

vec3 _vec3_nlerp(vec3 s, vec3 e, num t) {
	vec3 linear = new vec3(
		s.x + (e.x - s.x) * t,
		s.y + (e.y - s.y) * t,
		s.z + (e.z - s.z) * t
	);
	return _vec3_normalized(linear);
}

vec4 _vec4_nlerp(vec4 s, vec4 e, num t) {
	vec4 linear = new vec4(
		s.x + (e.x - s.x) * t,
		s.y + (e.y - s.y) * t,
		s.z + (e.z - s.z) * t,
        s.w + (e.w - s.w) * t
	);
	return _vec4_normalized(linear);
}

bool _vec2_compare(vec2 l, vec2 r) {
    num dx = l.x - r.x;
    num dy = l.y - r.y;
    num lenSq = dx * dx + dy * dy;
	return lenSq < vec2.epsilon;
}

bool _vec3_compare(vec3 l, vec3 r) {
    num dx = l.x - r.x;
    num dy = l.y - r.y;
    num dz = l.z - r.z;
    num lenSq = dx * dx + dy * dy + dz * dz;
	return lenSq < vec3.epsilon;
}

bool _vec4_compare(vec4 l, vec4 r) {
    num dx = l.x - r.x;
    num dy = l.y - r.y;
    num dz = l.z - r.z;
    num dw = l.w - r.w;
    num lenSq = dx * dx + dy * dy + dz * dz + dw * dw;
	return lenSq < vec4.epsilon;
}

delegate vec2 _vec2_flt(vec2 left, num right);
delegate num _vec2_num(vec2 vec);
delegate vec2 _vec2_bin(vec2 left, vec2 right);
delegate vec2 _vec2_interpolate(vec2 left, vec2 right, num t);
delegate num  _vec2_bin_num(vec2 left, vec2 right);
delegate vec2  _vec2_vec2(vec2 vec);
delegate bool  _vec2_bin_bool(vec2 left, vec2 right);

delegate vec3 _vec3_flt(vec3 left, num right);
delegate num _vec3_num(vec3 vec);
delegate vec3 _vec3_bin(vec3 left, vec3 right);
delegate vec3 _vec3_interpolate(vec3 left, vec3 right, num t);
delegate num  _vec3_bin_num(vec3 left, vec3 right);
delegate vec3  _vec3_vec3(vec3 vec);
delegate bool  _vec3_bin_bool(vec3 left, vec3 right);

delegate vec4 _vec4_flt(vec4 left, num right);
delegate num _vec4_num(vec4 vec);
delegate vec4 _vec4_bin(vec4 left, vec4 right);
delegate vec3 _vec4_bin_vec3(vec4 left, vec4 right);
delegate vec4 _vec4_interpolate(vec4 left, vec4 right, num t);
delegate num  _vec4_bin_num(vec4 left, vec4 right);
delegate vec4  _vec4_vec4(vec4 vec);
delegate bool  _vec4_bin_bool(vec4 left, vec4 right);

_vec2 vec2 = new _vec2();
_vec3 vec3 = new _vec3();
_vec3 vec4 = new _vec4();
""";
        protected static string QuaternionAPI =
"""
struct quat {
    num x = 0.0;
    num y = 0.0;
    num z = 0.0;
    num w = 1.0;
}

struct _quat {
    num epsilon = 0.0001;
    _qdel_fromAngleAxis     fromAngleAxis           = _quat_angleAxis;
    _qdel_fromTo            fromTo                  = _quat_fromTo;       
    _qdel_fromTo            look                    = _quat_lookRotation;       
    _qdel_getAxis           getAxis                 = _quat_getAxis;
    _quat_del_num           getAngle                = _quat_getAngle;
    _qdel_bin               add                     = _quat_add;
    _qdel_bin               sub                     = _quat_sub;
    _qdel_bin               mul                     = _quat_mul;
    _quat_del_quat_vec_vec  rotate                  = _quat_rotate; // rotates vec3
    _qdel_scale             scale                   = _quat_scale;
    _quat_del_quat          negate                  = _quat_negate;
    _qdel_compare           compare                 = _quat_compare;
    _qdel_compare           compareOrientation      = _quat_compare_orientation;
    _qdel_dot               dot                     = _quat_dot;
    _quat_del_num           lenSq                   = _quat_lenSq;
    _quat_del_num           len                     = _quat_len;
    _quat_del_quat          normalized              = _quat_normalized;
    _quat_del_quat          conjugate               = _quat_conjugate;
    _quat_del_quat          inverse                 = _quat_inverse;
    _quat_del_mix           mix                     = _quat_mix;
    _quat_del_mix           nlerp                   = _quat_nlerp;
    _quat_del_mix           slerp                   = _quat_slerp;
    _qdel_scale             pow                     = _quat_pow;
}


quat _quat_lookRotation(vec3 direcion, vec3 up) {
	// Find orthonormal basis vectors
	vec3 f = vec3.normalized(direcion);
	vec3 u = vec3.normalized(up);
	vec3 r = vec3.cross(u, f);
	u = vec3.cross(f, r);

    vec3 _up = new vec3(0, 1, 0);
    vec3 _forward = new vec3(0, 0, 1);

	// From world forward to object forward
	quat f2d = quat.fromTo(_forward, f);

	// what direction is the new object up?
	vec3 objectUp = f2d * _up;
	// From object up to desired up
	quat u2u = quat.fromTo(objectUp, u);

	// Rotate to forward direction first, then twist to correct up
	quat result = f2d * u2u;
	// Don’t forget to normalize the result
	return quat.normalized(result);
}

quat _quat_slerp(quat start, quat end, num t) {
	if (math.abs(quat.dot(start, end)) > 1.0 - quat.epsilon) {
		return quat.nlerp(start, end, t);
	}

	return quat.normalized(
        (
            quat.pow(
                quat.inverse(start) * end,  t
            )
        ) * start
    );
}

quat _quat_pow(quat q, num f) {
    vec3 vector = new vec3(q.x, q.y, q.z);
    num scalar = q.w;

	num angle = 2.0 * math.acos(scalar);
	vec3 axis = vec3.normalized(vector);

	num halfCos = math.cos(f * angle * 0.5);
	num halfSin = math.sin(f * angle * 0.5);

	return new quat(
		axis.x * halfSin,
		axis.y * halfSin,
		axis.z * halfSin,
		halfCos
	);
}

quat _quat_mix(quat from, quat to, num t) {
	return from * (1.0 - t) + to * t;
}

quat _quat_nlerp(quat from, quat to, num t) {
	return quat.normalized(from + (to - from) * t);
}

vec3 _quat_rotate(quat q, vec3 v) {
    vec3 vector = new vec3(q.x, q.y, q.z);
    num scalar = q.w;

	return vector * 2.0 * vec3.dot(vector, v) +
		v * (scalar * scalar - vec3.dot(vector, vector)) +
		vec3.cross(vector, v) * 2.0 * scalar;
}

quat _quat_mul(quat Q1, quat Q2) {
	return new quat(
		Q2.x * Q1.w + Q2.y * Q1.z - Q2.z * Q1.y + Q2.w * Q1.x,
		-Q2.x * Q1.z + Q2.y * Q1.w + Q2.z * Q1.x + Q2.w * Q1.y,
		Q2.x * Q1.y - Q2.y * Q1.x + Q2.z * Q1.w + Q2.w * Q1.z,
		-Q2.x * Q1.x - Q2.y * Q1.y - Q2.z * Q1.z + Q2.w * Q1.w
	);
}

quat _quat_normalized(quat q) {
	num lenSq = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
	if (lenSq < quat.epsilon) {
		return new quat();
	}
	num i_len = 1.0 / math.sqrt(lenSq);

	return new quat(
		q.x * i_len,
		q.y * i_len,
		q.z * i_len,
		q.w * i_len
	);
}

quat _quat_conjugate(quat q) {
	return new quat(
		-q.x,
		-q.y,
		-q.z,
		q.w
	);
}

quat _quat_inverse(quat q) {
	num lenSq = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
	if (lenSq < quat.epsilon) {
		return new quat();
	}
	num recip = 1.0 / lenSq;
	
    return new quat( // conjugate / norm
		-q.x * recip,
		-q.y * recip,
		-q.z * recip,
		 q.w * recip
	);
}

num _quat_dot(quat a, quat b) {
	return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
}

num _quat_lenSq(quat q) {
	return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
}

num _quat_len(quat q) {
	num lenSq = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
	if (lenSq < quat.epsilon) {
		return 0.0;
	}
	return math.sqrt(lenSq);
}

bool _quat_compare_orientation(quat left, quat right) {
	return (math.abs(left.x - right.x) <= quat.epsilon and math.abs(left.y - right.y) <= quat.epsilon and 
		    math.abs(left.z - right.z) <= quat.epsilon and math.abs(left.w - right.w)  <= quat.epsilon)
		or (math.abs(left.x + right.x) <= quat.epsilon and math.abs(left.y + right.y) <= quat.epsilon and 
			math.abs(left.z + right.z) <= quat.epsilon and math.abs(left.w + right.w)  <= quat.epsilon);
}

bool _quat_compare(quat left, quat right) {
	return (math.abs(left.x - right.x) <= quat.epsilon and 
		    math.abs(left.y - right.y) <= quat.epsilon and 
		    math.abs(left.z - right.z) <= quat.epsilon and 
		    math.abs(left.w - right.w) <= quat.epsilon);
}

quat _quat_scale(quat a, num b) {
	return new quat(
		a.x * b,
		a.y * b,
		a.z * b,
		a.w * b
	);
}

quat _quat_negate(quat q) {
	return new quat(
		-q.x,
		-q.y,
		-q.z,
		-q.w
	);
}

vec3 _quat_getAxis(quat quat) {
	return vec3.normalized(new vec3(quat.x, quat.y, quat.z));
}

num _quat_getAngle(quat quat) {
	return 2.0 * math.acos(quat.w);
}

quat _quat_add(quat a, quat b) {
	return new quat(
		a.x + b.x,
		a.y + b.y,
		a.z + b.z,
		a.w + b.w
	);
}

quat _quat_sub(quat a, quat b) {
	return new quat(
		a.x - b.x,
		a.y - b.y,
		a.z - b.z,
		a.w - b.w
	);
}

quat _quat_angleAxis(num angle, vec3 axis) {
	vec3 norm = vec3.normalized(axis);
	num s = math.sin(angle * 0.5);

	return new quat(
		norm.x * s,
		norm.y * s,
		norm.z * s,
		math.cos(angle * 0.5)
	);
}

quat _quat_fromTo(vec3 from, vec3 to) {
	vec3 f = vec3.normalized(from);
	vec3 t = vec3.normalized(to);

	if (f == t) {
		return new quat();
	}

	else if (f == t * -1.0) {
		vec3 ortho = new vec3(1, 0, 0);
		if (math.abs(f.y) < math.abs(f.x)) {
			ortho = new vec3(0, 1, 0);
		}
		if (math.abs(f.z) < math.abs(f.y) and 
            math.abs(f.z) < math.abs(f.x)) {
			ortho = new vec3(0, 0, 1);
		}

		vec3 axis = vec3.normalized(vec3.cross(f, ortho));
		return new quat(axis.x, axis.y, axis.z, 0);
	}

	vec3 half = vec3.normalized(f + t);
	vec3 axis = vec3.cross(f, half);

	return new quat(
		axis.x,
		axis.y,
		axis.z,
		vec3.dot(f, half)
	);
}

delegate quat   _qdel_fromAngleAxis(num angle, vec3 axis);
delegate quat   _qdel_fromTo(vec3 from, vec3 to);
delegate quat   _quat_del_quat(quat q);
delegate vec3   _qdel_getAxis(quat q);
delegate num    _quat_del_num(quat q);
delegate quat   _qdel_bin(quat a, quat b);
delegate quat   _qdel_scale(quat a, num b);
delegate bool   _qdel_compare(quat left, quat right);
delegate num    _qdel_dot(quat a, quat b);
delegate vec3   _quat_del_quat_vec_vec(quat q, vec3 v);
delegate quat   _quat_del_mix(quat from, quat to, num t);

_quat quat = new _quat();

/* TODO:
mat4 quatToMat4(const quat& q);
quat mat4ToQuat(const mat4& m);*/
""";
        protected static string MatrixAPI =
"""
struct mat4 {
    num xx; num xy; num xz; num xw;
    num yx; num yy; num yz; num yw;
    num zx; num zy; num zz; num zw;
    num tx; num ty; num tz; num tw;
}

struct _mat4 {
    _matdel_get      get = _mat4_get;
    _matdel_set      set = _mat4_set;
}

num _mat4_get(mat4 m, num index) {
    if (index == 0) {
        return m.xx;
    }
    else if (index == 1) {
        return m.xy;
    }
    else if (index == 2) {
        return m.xz;
    }
    else if (index == 3) {
        return m.xw;
    }
    else if (index == 4) {
        return m.yx;
    }
    else if (index == 5) {
        return m.yy;
    }
    else if (index == 6) {
        return m.yz;
    }
    else if (index == 7) {
        return m.yw;
    }
    else if (index == 8) {
        return m.zx;
    }
    else if (index == 9) {
        return m.zy;
    }
    else if (index == 10) {
        return m.zz;
    }
    else if (index == 11) {
        return m.zw;
    }
    else if (index == 12) {
        return m.tx;
    }
    else if (index == 13) {
        return m.ty;
    }
    else if (index == 14) {
        return m.tz;
    }
    else if (index == 15) {
        return m.tw;
    }

    assert(false, "mat4 invalid index: " + index);
    return 0.0;
}

void _mat4_set(mat4 m, num index, num val) {
    if (index == 0) {
        m.xx = val;
    }
    else if (index == 1) {
        m.xy = val;
    }
    else if (index == 2) {
        m.xz = val;
    }
    else if (index == 3) {
        m.xw = val;
    }
    else if (index == 4) {
        m.yx = val;
    }
    else if (index == 5) {
        m.yy = val;
    }
    else if (index == 6) {
        m.yz = val;
    }
    else if (index == 7) {
        m.yw = val;
    }
    else if (index == 8) {
        m.zx = val;
    }
    else if (index == 9) {
        m.zy = val;
    }
    else if (index == 10) {
        m.zz = val;
    }
    else if (index == 11) {
        m.zw = val;
    }
    else if (index == 12) {
        m.tx = val;
    }
    else if (index == 13) {
        m.ty = val;
    }
    else if (index == 14) {
        m.tz = val;
    }
    else if (index == 15) {
        m.tw = val;
    }
    else {
        assert(false, "mat4 invalid index: " + index);
    }
}

delegate num _matdel_get(mat4 m, num index);
delegate void _matdel_get(mat4 m, num index, num index);

_mat4 mat4 = new _mat4();
""";
        public static string InternalCode {
            get {
                return BuiltInFunctions + "\n" +
                    ArrayAPI + "\n" +
                    MapAPI + "\n" +
                    StringAPI + "\n" +
                    MathAPI + "\n" +
                    VectorAPI + "\n" +
                    QuaternionAPI;
            }
        }

        public static string ExternalCode {
            get {
                // TODO: IMPLEMENT FILE API
                // TODO: VEC2, VEC3, VEC4
                // TODO: MAT2, MAT3, MAT4
                // TODO: QUAT
                // TODO: RENDERING API
                // TODO: AUDIO API
                return FileAPI;
            }
        }
    }
}
