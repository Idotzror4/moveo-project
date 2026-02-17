import React, { useState, useEffect } from 'react';
import { authService } from '../services/authService';
import './Dashboard.css';

function Dashboard({ user, onLogout, onChangePreferences }) {
  const [news, setNews] = useState(null);
  const [prices, setPrices] = useState(null);
  const [aiInsight, setAIInsight] = useState(null);
  const [meme, setMeme] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [votes, setVotes] = useState({}); 

  useEffect(() => {
    loadDashboardData();
    loadUserVotes();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const [newsData, pricesData, insightData, memeData] = await Promise.all([
        authService.getMarketNews(),
        authService.getCoinPrices(),
        authService.getAIInsight(),
        authService.getMeme()
      ]);

      setNews(newsData);
      setPrices(pricesData);
      setAIInsight(insightData);
      setMeme(memeData);
    } catch (err) {
      setError('Failed to load dashboard data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadUserVotes = async () => {
    try {
      const votesData = await authService.getUserVotes();
      const votesMap = {};
      votesData.forEach(vote => {
        votesMap[vote.sectionType] = {
          isPositive: vote.isPositive,
          voted: true
        };
      });
      setVotes(votesMap);
    } catch (err) {
      console.error('Failed to load user votes:', err);
    }
  };

  const handleVote = async (sectionType, isPositive) => {
    try {
      await authService.submitVote(sectionType, isPositive);
      setVotes(prev => ({
        ...prev,
        [sectionType]: { isPositive, voted: true }
      }));
    } catch (err) {
      console.error('Failed to submit vote:', err);
      alert('Failed to submit vote. Please try again.');
    }
  };

  const handleChangeVote = (sectionType) => {
    setVotes(prev => ({
      ...prev,
      [sectionType]: { ...prev[sectionType], voted: false }
    }));
  };

  if (loading) {
    return (
      <div className="dashboard-container">
        <div className="loading">Loading dashboard...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="dashboard-container">
        <div className="error">{error}</div>
      </div>
    );
  }

  return (
    <div className="dashboard-container">
      <div className="dashboard-header">
        <h1>Welcome, {user.name}!</h1>
        <div className="header-buttons">
          <button onClick={onChangePreferences} className="change-preferences-btn">
            Change My Preferences
          </button>
          <button onClick={onLogout} className="logout-btn">Logout</button>
        </div>
      </div>

      <div className="dashboard-grid">
        <div className="dashboard-section">
          <h2>📰 Market News</h2>
          <div className="section-content">
            {news?.results && news.results.length > 0 ? (
              <ul className="news-list">
                {news.results.slice(0, 5).map((item, index) => {
                  let newsUrl = null;
                  
                  if (item.url && typeof item.url === 'string' && item.url.startsWith('http')) {
                    newsUrl = item.url;
                  } else if (item.source?.url && typeof item.source.url === 'string' && item.source.url.startsWith('http')) {
                    newsUrl = item.source.url;
                  } else if (item.link && typeof item.link === 'string' && item.link.startsWith('http')) {
                    newsUrl = item.link;
                  }
                  
                  if (!newsUrl && item.source?.domain) {
                    newsUrl = `https://${item.source.domain}`;
                  }
                  
                  const isValidUrl = newsUrl && 
                                    newsUrl !== '#' && 
                                    typeof newsUrl === 'string' &&
                                    (newsUrl.startsWith('http://') || newsUrl.startsWith('https://'));
                  
                  return (
                    <li key={index}>
                      {isValidUrl ? (
                        <a 
                          href={newsUrl} 
                          target="_blank" 
                          rel="noopener noreferrer"
                          className="news-link"
                          onClick={(e) => {
                            if (!newsUrl || newsUrl === '#') {
                              e.preventDefault();
                            }
                          }}
                        >
                          {item.title || 'News item'}
                        </a>
                      ) : (
                        <span className="news-text">{item.title || 'News item'}</span>
                      )}
                    </li>
                  );
                })}
              </ul>
            ) : (
              <p>No news available</p>
            )}
          </div>
          <div className="vote-section">
            {votes.MarketNews?.voted ? (
              <div className="vote-feedback">
                <p className="vote-thank-you">Thank you for your feedback!</p>
                <button className="change-vote-btn" onClick={() => handleChangeVote('MarketNews')}>
                  Change your vote
                </button>
              </div>
            ) : (
              <div className="vote-buttons">
                <button onClick={() => handleVote('MarketNews', true)}>👍</button>
                <button onClick={() => handleVote('MarketNews', false)}>👎</button>
              </div>
            )}
          </div>
        </div>

        <div className="dashboard-section">
          <h2>💰 Coin Prices</h2>
          <div className="section-content">
            {prices ? (
              <div className="prices-list">
                {Object.entries(prices).map(([coin, data]) => (
                  <div key={coin} className="price-item">
                    <strong>{coin.charAt(0).toUpperCase() + coin.slice(1)}:</strong> ${data.usd?.toLocaleString() || 'N/A'}
                  </div>
                ))}
              </div>
            ) : (
              <p>No prices available</p>
            )}
          </div>
          <div className="vote-section">
            {votes.CoinPrices?.voted ? (
              <div className="vote-feedback">
                <p className="vote-thank-you">Thank you for your feedback!</p>
                <button className="change-vote-btn" onClick={() => handleChangeVote('CoinPrices')}>
                  Change your vote
                </button>
              </div>
            ) : (
              <div className="vote-buttons">
                <button onClick={() => handleVote('CoinPrices', true)}>👍</button>
                <button onClick={() => handleVote('CoinPrices', false)}>👎</button>
              </div>
            )}
          </div>
        </div>

        <div className="dashboard-section">
          <h2>🤖 AI Insight of the Day</h2>
          <div className="section-content">
            {aiInsight?.insight ? (
              <p className="ai-insight">{aiInsight.insight}</p>
            ) : (
              <p>No insight available</p>
            )}
          </div>
          <div className="vote-section">
            {votes.AIInsight?.voted ? (
              <div className="vote-feedback">
                <p className="vote-thank-you">Thank you for your feedback!</p>
                <button className="change-vote-btn" onClick={() => handleChangeVote('AIInsight')}>
                  Change your vote
                </button>
              </div>
            ) : (
              <div className="vote-buttons">
                <button onClick={() => handleVote('AIInsight', true)}>👍</button>
                <button onClick={() => handleVote('AIInsight', false)}>👎</button>
              </div>
            )}
          </div>
        </div>

        <div className="dashboard-section">
          <h2>😄 Fun Crypto Meme</h2>
          <div className="section-content">
            {meme ? (
                <div className="meme-content">
                  <h3>{meme.title}</h3>
                  {meme.url && meme.url.startsWith('http') ? (
                    <img 
                      src={meme.url} 
                      alt={meme.title} 
                      className="meme-image" 
                      style={{ maxWidth: '100%', height: 'auto', borderRadius: '8px' }}
                      onError={(e) => {
                        e.target.style.display = 'none';
                      }} 
                    />
                  ) : (
                    <div className="meme-text">
                      <p style={{ fontSize: '2em', margin: '20px 0', color: '#666' }}>📷</p>
                      <p style={{ color: '#666', fontStyle: 'italic' }}>{meme.title}</p>
                    </div>
                  )}
                </div>
            ) : (
              <p>No meme available</p>
            )}
          </div>
          <div className="vote-section">
            {votes.Meme?.voted ? (
              <div className="vote-feedback">
                <p className="vote-thank-you">Thank you for your feedback!</p>
                <button className="change-vote-btn" onClick={() => handleChangeVote('Meme')}>
                  Change your vote
                </button>
              </div>
            ) : (
              <div className="vote-buttons">
                <button onClick={() => handleVote('Meme', true)}>👍</button>
                <button onClick={() => handleVote('Meme', false)}>👎</button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;