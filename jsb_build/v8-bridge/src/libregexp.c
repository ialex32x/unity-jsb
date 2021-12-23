
#ifdef UNITY_EXT_COMPILING

#ifndef countof
#define countof(x) (sizeof(x) / sizeof((x)[0]))
#endif

/* code point ranges for Zs,Zl or Zp property */
static const uint16_t char_range_s[] = {
    10,
    0x0009, 0x000D + 1,
    0x0020, 0x0020 + 1,
    0x00A0, 0x00A0 + 1,
    0x1680, 0x1680 + 1,
    0x2000, 0x200A + 1,
    /* 2028;LINE SEPARATOR;Zl;0;WS;;;;;N;;;;; */
    /* 2029;PARAGRAPH SEPARATOR;Zp;0;B;;;;;N;;;;; */
    0x2028, 0x2029 + 1,
    0x202F, 0x202F + 1,
    0x205F, 0x205F + 1,
    0x3000, 0x3000 + 1,
    /* FEFF;ZERO WIDTH NO-BREAK SPACE;Cf;0;BN;;;;;N;BYTE ORDER MARK;;;; */
    0xFEFF, 0xFEFF + 1,
};

JS_BOOL lre_is_space(int c)
{
    int i, n, low, high;
    n = (countof(char_range_s) - 1) / 2;
    for (i = 0; i < n; i++) {
        low = char_range_s[2 * i + 1];
        if (c < low)
            return FALSE;
        high = char_range_s[2 * i + 2];
        if (c < high)
            return TRUE;
    }
    return FALSE;
}

#endif
