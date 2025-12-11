import React, { useState } from 'react';
import { Card, Button, InputNumber, Alert, Spin, Typography, Divider, List, Tag } from 'antd';
import { UploadOutlined, DiffOutlined } from '@ant-design/icons';
import axios from 'axios';

const { Text } = Typography;

interface BasicInfoDiff {
  IsMatch: boolean;
  VendorIdExpected: number;
  VendorIdActual: number;
  VendorIdMatch: boolean;
  ProductCodeExpected: number;
  ProductCodeActual: number;
  ProductCodeMatch: boolean;
  RevisionNumberExpected: number;
  RevisionNumberActual: number;
  RevisionNumberMatch: boolean;
}

interface SectionDiff {
  HasDifferences: boolean;
  Differences: string[];
  TotalObjects?: number;
  TotalSubIndices?: number;
  TotalSyncManagers?: number;
  TotalPDOMappings?: number;
  TotalPDOEntries?: number;
}

interface ConfigDiffResult {
  HasDifferences: boolean;
  BasicInfoDiff: BasicInfoDiff;
  ObjectDictionaryDiff: SectionDiff;
  SyncManagerDiff: SectionDiff;
  PDOMappingDiff: SectionDiff;
  OverallDifferences: string[];
}

const ConfigComparison: React.FC = () => {
  const [file, setFile] = useState<File | null>(null);
  const [vendorId, setVendorId] = useState<number | null>(null);
  const [productCode, setProductCode] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<ConfigDiffResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files && e.target.files[0];
    setFile(f || null);
    setError(null);
  };

  const handleCompare = async () => {
    if (!file) {
      setError('请先选择ESI文件');
      return;
    }
    if (vendorId === null || productCode === null) {
      setError('请输入实际厂商ID和产品代码');
      return;
    }

    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('actualVendorId', String(vendorId));
      formData.append('actualProductCode', String(productCode));

      const response = await axios.post<ConfigDiffResult>('http://localhost:5271/api/ConfigComparison/compare', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      setResult(response.data);
    } catch (err: any) {
      setError(`配置对比失败: ${err.response?.data?.message || err.message}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="配置对比" bordered={false}>
      {error && (
        <Alert message="错误" description={error} type="error" showIcon style={{ marginBottom: 16 }} />
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 16 }}>
        <div>
          <input type="file" accept=".xml" style={{ display: 'none' }} id="compare-file-input" onChange={handleInputChange} />
          <label htmlFor="compare-file-input">
            <Button icon={<UploadOutlined />}>选择ESI文件</Button>
          </label>
          <Text type="secondary" style={{ marginLeft: 16 }}>XML格式ESI文件</Text>
        </div>
        <div style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
          <div>
            <Text strong>实际厂商ID</Text>
            <InputNumber min={0} max={0xFFFF} style={{ width: 160 }} value={vendorId as number | null} onChange={setVendorId} />
          </div>
          <div>
            <Text strong>实际产品代码</Text>
            <InputNumber min={0} max={0xFFFFFFFF} style={{ width: 200 }} value={productCode as number | null} onChange={setProductCode} />
          </div>
          <Button type="primary" icon={<DiffOutlined />} onClick={handleCompare} loading={loading}>开始对比</Button>
        </div>
      </div>

      {loading && (
        <div style={{ textAlign: 'center', padding: 24 }}>
          <Spin size="large" />
          <p style={{ marginTop: 16 }}>处理中...</p>
        </div>
      )}

      {result && (
        <div>
          <Divider orientation="left">基础信息</Divider>
          <Card size="small" style={{ marginBottom: 16 }}>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 16 }}>
              <div>
                <Text strong>厂商ID 期望</Text>
                <br />
                <Tag color={result.BasicInfoDiff.VendorIdMatch ? 'green' : 'red'}>
                  0x{result.BasicInfoDiff.VendorIdExpected.toString(16).toUpperCase().padStart(4, '0')}
                </Tag>
              </div>
              <div>
                <Text strong>厂商ID 实际</Text>
                <br />
                <Tag>
                  0x{result.BasicInfoDiff.VendorIdActual.toString(16).toUpperCase().padStart(4, '0')}
                </Tag>
              </div>
              <div>
                <Text strong>产品代码 期望</Text>
                <br />
                <Tag color={result.BasicInfoDiff.ProductCodeMatch ? 'green' : 'red'}>
                  0x{result.BasicInfoDiff.ProductCodeExpected.toString(16).toUpperCase().padStart(8, '0')}
                </Tag>
              </div>
              <div>
                <Text strong>产品代码 实际</Text>
                <br />
                <Tag>
                  0x{result.BasicInfoDiff.ProductCodeActual.toString(16).toUpperCase().padStart(8, '0')}
                </Tag>
              </div>
            </div>
          </Card>

          <Divider orientation="left">对象字典差异</Divider>
          <Card size="small" style={{ marginBottom: 16 }}>
            <div style={{ marginBottom: 8 }}>
              <Text>对象总数: {result.ObjectDictionaryDiff.TotalObjects || 0}</Text>
              <span style={{ marginLeft: 16 }} />
              <Text>子索引总数: {result.ObjectDictionaryDiff.TotalSubIndices || 0}</Text>
            </div>
            <List
              dataSource={result.ObjectDictionaryDiff.Differences}
              renderItem={(item) => <List.Item>{item}</List.Item>}
              locale={{ emptyText: '无差异' }}
            />
          </Card>

          <Divider orientation="left">同步管理器差异</Divider>
          <Card size="small" style={{ marginBottom: 16 }}>
            <div style={{ marginBottom: 8 }}>
              <Text>SM数量: {result.SyncManagerDiff.TotalSyncManagers || 0}</Text>
            </div>
            <List
              dataSource={result.SyncManagerDiff.Differences}
              renderItem={(item) => <List.Item>{item}</List.Item>}
              locale={{ emptyText: '无差异' }}
            />
          </Card>

          <Divider orientation="left">PDO映射差异</Divider>
          <Card size="small" style={{ marginBottom: 16 }}>
            <div style={{ marginBottom: 8 }}>
              <Text>PDO映射数: {result.PDOMappingDiff.TotalPDOMappings || 0}</Text>
              <span style={{ marginLeft: 16 }} />
              <Text>PDO条目数: {result.PDOMappingDiff.TotalPDOEntries || 0}</Text>
            </div>
            <List
              dataSource={result.PDOMappingDiff.Differences}
              renderItem={(item) => <List.Item>{item}</List.Item>}
              locale={{ emptyText: '无差异' }}
            />
          </Card>

          <Divider orientation="left">总体结论</Divider>
          <Alert
            message={result.HasDifferences ? '发现差异' : '无差异'}
            description={result.HasDifferences ? '配置存在差异，请核查上述列表' : 'ESI与实际设备信息一致'}
            type={result.HasDifferences ? 'warning' : 'success'}
            showIcon
          />
        </div>
      )}
    </Card>
  );
};

export default ConfigComparison;
