import React, { createContext, useContext, useState, useEffect } from 'react';
import { ConfigProvider, theme } from 'antd';

const ThemeContext = createContext();
export const useTheme = () => useContext(ThemeContext);

export const AppThemeProvider = ({ children }) => {
    // states: 'light', 'dark', 'system'
    const [themeMode, setThemeMode] = useState(localStorage.getItem('themeMode') || 'system');
    
    // actual boolean value to apply
    const [isDarkMode, setIsDarkMode] = useState(false);

    useEffect(() => {
        const updateTheme = () => {
            if (themeMode === 'system') {
                setIsDarkMode(window.matchMedia('(prefers-color-scheme: dark)').matches);
            } else {
                setIsDarkMode(themeMode === 'dark');
            }
        };

        updateTheme();
        
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        const listener = (e) => {
            if (themeMode === 'system') setIsDarkMode(e.matches);
        };
        
        mediaQuery.addEventListener('change', listener);
        return () => mediaQuery.removeEventListener('change', listener);
    }, [themeMode]);

    const changeTheme = (mode) => {
        setThemeMode(mode);
        localStorage.setItem('themeMode', mode);
    };

    // The Hawaii Vibe Colors: 
    // #9B8EC7 (Lilac), #BDA6CE (Soft Lavender), #B4D3D9 (Ocean Foam), #F2EAE0 (Sand)
    
    return (
        <ThemeContext.Provider value={{ themeMode, isDarkMode, changeTheme }}>
            <ConfigProvider 
                theme={{
                    algorithm: isDarkMode ? theme.darkAlgorithm : theme.defaultAlgorithm,
                    token: {
                        colorPrimary: '#9B8EC7',
                        colorInfo: '#B4D3D9',
                        colorBgBase: isDarkMode ? '#1a1a24' : '#F2EAE0',
                        colorBgContainer: isDarkMode ? '#232231' : '#ffffff',
                        fontFamily: '"Inter", sans-serif',
                        borderRadius: 12,
                    },
                    components: {
                        Typography: {
                            // Target all titles with the Hawaii/Cursive font
                            titleMarginBottom: '1em',
                        },
                        Card: {
                            colorBgContainer: isDarkMode ? '#232231' : '#ffffff',
                        }
                    }
                }}
            >
                <div style={{ backgroundColor: isDarkMode ? '#1a1a24' : '#F2EAE0', minHeight: '100vh', width: '100%' }}>
                    {children}
                </div>
            </ConfigProvider>
        </ThemeContext.Provider>
    );
};
