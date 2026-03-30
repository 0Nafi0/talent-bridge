import { Menu, Button, Space, Dropdown, theme } from 'antd';
import { HomeOutlined, UserOutlined, DashboardOutlined, LoginOutlined, LogoutOutlined, BgColorsOutlined, SunOutlined, MoonOutlined, DesktopOutlined } from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';

const Navbar = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { user, logout } = useAuth();
    const { themeMode, changeTheme, isDarkMode } = useTheme();
    const { token } = theme.useToken();

    const handleLogout = () => {
        logout();
        navigate('/');
    };

    const themeItems = [
        { key: 'light', icon: <SunOutlined />, label: 'Light Theme' },
        { key: 'dark', icon: <MoonOutlined />, label: 'Dark Theme' },
        { key: 'system', icon: <DesktopOutlined />, label: 'System Theme' },
    ];

    const navItems = [
        { key: '/', icon: <HomeOutlined />, label: 'Home' },
        { key: '/jobs', icon: <DashboardOutlined />, label: 'Job Board' }
    ];

    if (user?.role === 'Candidate') navItems.push({ key: '/dashboard/candidate', icon: <DashboardOutlined />, label: 'Candidate Dashboard' });
    if (user?.role === 'Recruiter' || user?.role === 'Admin') navItems.push({ key: '/dashboard/recruiter', icon: <DashboardOutlined />, label: 'Recruiter Dashboard' });

    return (
        <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center', 
            padding: '0 24px', 
            background: isDarkMode ? 'rgba(35, 34, 49, 0.7)' : 'rgba(255, 255, 255, 0.7)',
            backdropFilter: 'blur(12px)',
            WebkitBackdropFilter: 'blur(12px)',
            border: `1px solid ${isDarkMode ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.05)'}`,
            boxShadow: '0 4px 30px rgba(0, 0, 0, 0.1)',
            borderRadius: '100px', // Circular pills look
            position: 'sticky',
            top: 20,
            zIndex: 1000,
            maxWidth: '1100px',
            margin: '20px auto',
            width: 'calc(100% - 40px)',
            transition: 'all 0.3s ease'
        }}>
            <style>
                @import url('https://fonts.googleapis.com/css2?family=Pacifico&display=swap');
                {`
                    h1, h2, h3, h4, h5, .ant-typography {
                        font-family: 'Inter', sans-serif;
                    }
                    /* Apply Hawaii cursive only to main elegant titles */
                    .hawaii-title {
                        font-family: 'Pacifico', cursive !important;
                        font-weight: 400;
                    }
                    .ant-menu-horizontal {
                        border-bottom: none !important;
                    }
                `}
            </style>

            <div 
                className="hawaii-title"
                style={{ fontSize: '26px', cursor: 'pointer', color: token.colorPrimary, paddingTop: '4px' }} 
                onClick={() => navigate('/')}
            >
                TalentBridge
            </div>
            
            <Menu 
                mode="horizontal" 
                selectedKeys={[location.pathname]} 
                items={navItems}
                onClick={({ key }) => navigate(key)}
                style={{ flex: 1, border: 'none', marginLeft: '40px', background: 'transparent' }}
            />

            <Space>
                <Dropdown menu={{ items: themeItems, onClick: ({key}) => changeTheme(key), selectedKeys: [themeMode] }} placement="bottomRight">
                    <Button type="text" shape="circle" icon={<BgColorsOutlined />} />
                </Dropdown>

                {user ? (
                    <>
                        <span style={{ marginRight: '16px', color: token.colorTextSecondary }}>
                            <UserOutlined style={{ marginRight: '8px' }} />
                            {user.fullName}
                        </span>
                        <Button type="primary" danger icon={<LogoutOutlined />} onClick={handleLogout}>Logout</Button>
                    </>
                ) : (
                    <>
                        <Button type="default" icon={<LoginOutlined />} onClick={() => navigate('/login')}>Login</Button>
                        <Button type="primary" onClick={() => navigate('/register')}>Sign Up</Button>
                    </>
                )}
            </Space>
        </div>
    );
};

export default Navbar;
