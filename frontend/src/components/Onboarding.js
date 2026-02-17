import React, { useState, useEffect } from 'react';
import { authService } from '../services/authService';
import './Onboarding.css';

function Onboarding({ onComplete }) {
  const [interestedAssets, setInterestedAssets] = useState([]);
  const [investorType, setInvestorType] = useState('');
  const [contentTypes, setContentTypes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [loadingPreferences, setLoadingPreferences] = useState(true);
  const [error, setError] = useState('');

  const cryptoAssets = ['Bitcoin', 'Ethereum', 'Solana', 'Cardano', 'Polygon', 'Other'];
  const investorTypes = ['HODLer', 'Day Trader', 'NFT Collector'];
  const contentTypesOptions = ['Market News', 'Charts', 'Social', 'Fun'];

  useEffect(() => {
    const loadPreferences = async () => {
      try {
        const preferences = await authService.getPreferences();
        
        if (preferences.interestedAssets) {
          try {
            const assets = JSON.parse(preferences.interestedAssets);
            setInterestedAssets(Array.isArray(assets) ? assets : []);
          } catch {
            setInterestedAssets([]);
          }
        }
        
        if (preferences.investorType) {
          setInvestorType(preferences.investorType);
        }
        
        if (preferences.contentTypes) {
          try {
            const types = JSON.parse(preferences.contentTypes);
            setContentTypes(Array.isArray(types) ? types : []);
          } catch {
            setContentTypes([]);
          }
        }
      } catch (err) {
        if (err.message !== 'NOT_FOUND' && err.response?.status !== 404) {
          console.error('Error loading preferences:', err);
        }
      } finally {
        setLoadingPreferences(false);
      }
    };

    loadPreferences();
  }, []);

  const handleAssetToggle = (asset) => {
    if (interestedAssets.includes(asset)) {
      setInterestedAssets(interestedAssets.filter(a => a !== asset));
    } else {
      setInterestedAssets([...interestedAssets, asset]);
    }
  };

  const handleContentTypeToggle = (type) => {
    if (contentTypes.includes(type)) {
      setContentTypes(contentTypes.filter(t => t !== type));
    } else {
      setContentTypes([...contentTypes, type]);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await authService.savePreferences(
        JSON.stringify(interestedAssets),
        investorType,
        JSON.stringify(contentTypes)
      );
      onComplete();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save preferences');
    } finally {
      setLoading(false);
    }
  };

  if (loadingPreferences) {
    return (
      <div className="onboarding-container">
        <div className="loading">Loading your preferences...</div>
      </div>
    );
  }

  return (
    <div className="onboarding-container">
      <h1>Welcome! Let's personalize your experience</h1>
      <form onSubmit={handleSubmit}>
        <div className="question-section">
          <h3>What crypto assets are you interested in?</h3>
          <div className="options-grid">
            {cryptoAssets.map(asset => (
              <label key={asset} className="option-checkbox">
                <input
                  type="checkbox"
                  checked={interestedAssets.includes(asset)}
                  onChange={() => handleAssetToggle(asset)}
                />
                <span>{asset}</span>
              </label>
            ))}
          </div>
        </div>

        <div className="question-section">
          <h3>What type of investor are you?</h3>
          <div className="options-radio">
            {investorTypes.map(type => (
              <label key={type} className="option-radio">
                <input
                  type="radio"
                  name="investorType"
                  value={type}
                  checked={investorType === type}
                  onChange={(e) => setInvestorType(e.target.value)}
                />
                <span>{type}</span>
              </label>
            ))}
          </div>
        </div>

        <div className="question-section">
          <h3>What kind of content would you like to see?</h3>
          <div className="options-grid">
            {contentTypesOptions.map(type => (
              <label key={type} className="option-checkbox">
                <input
                  type="checkbox"
                  checked={contentTypes.includes(type)}
                  onChange={() => handleContentTypeToggle(type)}
                />
                <span>{type}</span>
              </label>
            ))}
          </div>
        </div>

        {error && <div className="error">{error}</div>}
        <button type="submit" disabled={loading || !investorType || interestedAssets.length === 0 || contentTypes.length === 0}>
          {loading ? 'Saving...' : 'Continue'}
        </button>
      </form>
    </div>
  );
}

export default Onboarding;