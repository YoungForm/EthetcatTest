import React, { useState } from 'react';
import { Button, Card, Alert, Spin, Typography, Divider } from 'antd';
import { UploadOutlined, FileTextOutlined, CheckCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import axios from 'axios';

const { Title, Text } = Typography;

interface ESIInfo {
  VendorId: number;
  ProductCode: number;
  RevisionNo: number;
  OrderCode: string;
  XmlVersion: string;
  ObjectDictionary?: any[];
  SyncManagers?: any[];
  PDOMappings?: any[];
}

interface ESIParseResult {
  IsValid?: boolean;
  ValidationErrors?: string;
  IsConsistent?: boolean;
  ESIInfo?: ESIInfo;
  ActualVendorId?: number;
  ActualProductCode?: number;
}

const ESIUploader: React.FC = () => {
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [parseResult, setParseResult] = useState<ESIParseResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files && e.target.files[0];
    if (f) {
      setFile(f);
      setError(null);
    }
  };

  const handleParse = async () => {
    if (!file) {
      setError('请先选择ESI文件');
      return;
    }

    setLoading(true);
    setError(null);
    setParseResult(null);

    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await axios.post<ESIInfo>('http://localhost:5271/api/ESI/Parse', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      setParseResult({ ESIInfo: response.data });
    } catch (err: any) {
      setError(`解析失败: ${err.response?.data || err.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleValidate = async () => {
    if (!file) {
      setError('请先选择ESI文件');
      return;
    }

    setLoading(true);
    setError(null);
    setParseResult(null);

    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await axios.post<ESIParseResult>('http://localhost:5271/api/ESI/Validate', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      setParseResult(response.data);
    } catch (err: any) {
      setError(`验证失败: ${err.response?.data || err.message}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="ESI文件上传与解析" bordered={false}>
      {error && (
        <Alert
          message="错误"
          description={error}
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      <div style={{ marginBottom: 16 }}>
        <input type="file" accept=".xml" style={{ display: 'none' }} id="esi-file-input" onChange={handleInputChange} />
        <label htmlFor="esi-file-input">
          <Button icon={<UploadOutlined />}>选择ESI文件</Button>
        </label>
        <Text type="secondary" style={{ marginLeft: 16 }}>
          支持XML格式的ESI文件
        </Text>
      </div>

      <div style={{ display: 'flex', gap: 16, marginBottom: 24 }}>
        <Button
          type="primary"
          icon={<FileTextOutlined />}
          onClick={handleParse}
          loading={loading}
        >
          解析ESI文件
        </Button>
        <Button
          type="default"
          icon={<CheckCircleOutlined />}
          onClick={handleValidate}
          loading={loading}
        >
          验证ESI文件
        </Button>
      </div>

      {loading && (
        <div style={{ textAlign: 'center', padding: 24 }}>
          <Spin size="large" />
          <p style={{ marginTop: 16 }}>处理中...</p>
        </div>
      )}

      {parseResult && (
        <div>
          <Divider orientation="left">ESI文件信息</Divider>
          
          {parseResult.ESIInfo && (
            <Card size="small" style={{ marginBottom: 16 }}>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16 }}>
                <div>
                  <Text strong>厂商ID:</Text>
                  <br />
                  <Text>{parseResult.ESIInfo.VendorId.toString(16).toUpperCase().padStart(4, '0')}</Text>
                </div>
                <div>
                  <Text strong>产品代码:</Text>
                  <br />
                  <Text>{parseResult.ESIInfo.ProductCode.toString(16).toUpperCase().padStart(8, '0')}</Text>
                </div>
                <div>
                  <Text strong>版本号:</Text>
                  <br />
                  <Text>{parseResult.ESIInfo.RevisionNo.toString(16).toUpperCase().padStart(4, '0')}</Text>
                </div>
                <div>
                  <Text strong>订单代码:</Text>
                  <br />
                  <Text>{parseResult.ESIInfo.OrderCode}</Text>
                </div>
                <div>
                  <Text strong>XML版本:</Text>
                  <br />
                  <Text>{parseResult.ESIInfo.XmlVersion}</Text>
                </div>
              </div>
            </Card>
          )}

          {parseResult.IsValid !== undefined && (
            <Card size="small" style={{ marginBottom: 16 }}>
              <Alert
                message={parseResult.IsValid ? '文件验证通过' : '文件验证失败'}
                description={parseResult.IsValid ? 'ESI文件符合XML Schema规范' : parseResult.ValidationErrors}
                type={parseResult.IsValid ? 'success' : 'error'}
                showIcon
              />
            </Card>
          )}

          {parseResult.IsConsistent !== undefined && (
            <Card size="small">
              <Alert
                message={parseResult.IsConsistent ? '设备信息一致' : '设备信息不一致'}
                description={`ESI文件中的设备信息与实际设备${parseResult.IsConsistent ? '' : '不'}匹配`}
                type={parseResult.IsConsistent ? 'success' : 'warning'}
                showIcon
              />
            </Card>
          )}
        </div>
      )}
    </Card>
  );
};

export default ESIUploader;
