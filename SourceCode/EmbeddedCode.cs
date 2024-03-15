namespace CScript {
    public class Embedded {
        protected static string BuiltInFunctions =
"""
// BUILT IN FUNCTIONS
delegate void _print(string str);
_print print = null;
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

num _vec2_dot(vec2 left, vec2 right) {
    return left.x * right.x + left.y * right.y;
}

num _vec3_dot(vec3 left, vec3 right) {
    return left.x * right.x + left.y * right.y + left.z * right.z;
}

num _vec2_lenSq(vec2 v) {
    return v.x * v.x + v.y * v.y;
}

num _vec3_lenSq(vec3 v) {
    return v.x * v.x + v.y * v.y + v.z * v.z;
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

vec2 _vec2_reject(vec2 a, vec2 b) {
	vec2 projection = _vec2_project(a, b);
	return a - projection;
}

vec3 _vec3_reject(vec3 a, vec3 b) {
	vec3 projection = _vec3_project(a, b);
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

vec3 _vec3_cross(vec3 l, vec3 r) {
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

_vec2 vec2 = new _vec2();
_vec3 vec3 = new _vec3();
""";
        public static string InternalCode {
            get {
                return BuiltInFunctions + "\n" +
                    ArrayAPI + "\n" +
                    MapAPI + "\n" +
                    StringAPI + "\n" +
                    MathAPI + "\n" +
                    VectorAPI;
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
