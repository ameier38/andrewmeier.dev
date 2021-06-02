module.exports = {
    mode: 'jit',
    purge: ['./static/**/*.html', './compiled/**/*.js'],
    plugins: [
        require('@tailwindcss/typography')
    ],
    theme: {
        extend: {
            typography: {
                DEFAULT: {
                    css: {
                        pre: false,
                        code: false,
                        'pre code': false,
                        'code::before': false,
                        'code::after': false,
                        'blockquote p:first-of-type::before': false,
                        'blockquote p:last-of-type::after': false,
                    },
                },
            },
        }
    },
};
