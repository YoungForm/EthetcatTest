import { Layout, Menu, theme, Card, Statistic, Button, Progress } from 'antd';
import React from 'react';
import { Routes, Route, Link, useLocation, useNavigate } from 'react-router-dom';
import './App.css';

import ESIUploader from './components/ESIUploader';
import ConfigComparison from './components/ConfigComparison';

// Import page components
const Dashboard = () => {
  // State for device connection status
  const [connectionStatus, setConnectionStatus] = React.useState(false);
  const [testProgress, setTestProgress] = React.useState(0);
  const navigate = useNavigate();
  
  // Mock connection function
  const toggleConnection = () => {
    setConnectionStatus(!connectionStatus);
  };
  
  return (
    <div style={{ padding: '24px', background: '#fff', minHeight: '280px' }}>
      <h2>仪表板</h2>
      <p>欢迎使用ETG自检工具</p>
      
      <div style={{ marginTop: '24px', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '16px' }}>
        {/* Device Connection Status */}
        <Card title="设备连接状态" bordered={false}>
          <Statistic
            title="连接状态"
            value={connectionStatus}
            valueStyle={{ color: connectionStatus ? '#3f8600' : '#cf1322' }}
            formatter={(value) => value ? '已连接' : '未连接'}
          />
          <Button 
            type={connectionStatus ? 'primary' : 'default'}
            danger={connectionStatus}
            onClick={toggleConnection}
            style={{ marginTop: '16px' }}
          >
            {connectionStatus ? '断开连接' : '连接设备'}
          </Button>
        </Card>
        
        {/* Test Progress */}
        <Card title="测试进度" bordered={false}>
          <Statistic
            title="完成率"
            value={testProgress}
            suffix="%"
          />
          <Progress percent={testProgress} status="active" style={{ marginTop: '16px' }} />
        </Card>
        
        {/* Test Results Summary */}
        <Card title="测试结果概览" bordered={false}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '16px' }}>
            <Statistic title="通过" value={85} valueStyle={{ color: '#3f8600' }} />
            <Statistic title="失败" value={5} valueStyle={{ color: '#cf1322' }} />
            <Statistic title="总计" value={90} />
          </div>
        </Card>
        
        {/* Device Information */}
        <Card title="设备信息" bordered={false}>
          <div style={{ marginTop: '16px' }}>
            <p><strong>设备类型:</strong> EtherCAT从站</p>
            <p><strong>Vendor ID:</strong> 0x0000</p>
            <p><strong>Product Code:</strong> 0x00000000</p>
          </div>
        </Card>
      </div>
      
      {/* Quick Actions */}
      <div style={{ marginTop: '24px' }}>
        <Card title="快速操作" bordered={false}>
          <div style={{ display: 'flex', gap: '16px' }}>
            <Button type="primary" size="large">
              一键测试
            </Button>
            <Button size="large" onClick={() => navigate('/config')}>
              上传ESI文件
            </Button>
            <Button size="large">
              查看测试报告
            </Button>
          </div>
        </Card>
      </div>
    </div>
  );
};

const DeviceConfig = () => (
  <div style={{ padding: '24px', background: '#fff', minHeight: '280px' }}>
    <h2>设备配置</h2>
    <p>ESI文件上传和解析</p>
    <ESIUploader />
  </div>
);

const TestExecution = () => <div style={{ padding: '24px', background: '#fff', minHeight: '280px' }}><h2>测试执行</h2><p>运行EtherCAT测试</p></div>;
const Results = () => <div style={{ padding: '24px', background: '#fff', minHeight: '280px' }}><h2>测试结果</h2><p>查看测试报告</p></div>;
const ComparePage = () => (
  <div style={{ padding: '24px', background: '#fff', minHeight: '280px' }}>
    <h2>配置对比</h2>
    <ConfigComparison />
  </div>
);

const { Header, Content, Sider } = Layout;

function App() {
  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken();
  
  const location = useLocation();

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        breakpoint="lg"
        collapsedWidth="0"
        style={{
          overflow: 'auto',
          height: '100vh',
          position: 'fixed',
          left: 0,
          top: 0,
          bottom: 0,
        }}
      >
        <div style={{ height: '32px', margin: '16px', background: 'rgba(255, 255, 255, 0.2)' }} />
        <Menu
          theme="dark"
          mode="inline"
          defaultSelectedKeys={['dashboard']}
          selectedKeys={[location.pathname.substring(1) || 'dashboard']}
          items={[
            {
              key: 'dashboard',
              icon: <span>📊</span>,
              label: <Link to="/">仪表板</Link>,
            },
            {
              key: 'config',
              icon: <span>⚙️</span>,
              label: <Link to="/config">设备配置</Link>,
            },
            {
              key: 'compare',
              icon: <span>🧩</span>,
              label: <Link to="/compare">配置对比</Link>,
            },
            {
              key: 'test',
              icon: <span>🧪</span>,
              label: <Link to="/test">测试执行</Link>,
            },
            {
              key: 'results',
              icon: <span>📋</span>,
              label: <Link to="/results">测试结果</Link>,
            },
          ]}
        />
      </Sider>
      <Layout style={{ marginLeft: 200 }}>
        <Header style={{ padding: 0, background: colorBgContainer }} />
        <Content style={{ margin: '24px 16px 0', overflow: 'initial' }}>
          <div
            style={{
              padding: 24,
              background: colorBgContainer,
              borderRadius: borderRadiusLG,
            }}
          >
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/config" element={<DeviceConfig />} />
              <Route path="/compare" element={<ComparePage />} />
              <Route path="/test" element={<TestExecution />} />
              <Route path="/results" element={<Results />} />
            </Routes>
          </div>
        </Content>
      </Layout>
    </Layout>
  );
}

export default App;
