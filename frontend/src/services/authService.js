import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';
const API_URL = `${API_BASE_URL}/api/auth`;

export const authService = {
  register: async (email, name, password) => {
    const response = await axios.post(`${API_URL}/register`, {
      email,
      name,
      password
    });
    return response.data;
  },

  login: async (email, password) => {
    const response = await axios.post(`${API_URL}/login`, {
      email,
      password
    });
    return response.data;
  },

  savePreferences: async (interestedAssets, investorType, contentTypes) => {
    const token = localStorage.getItem('token');
    const response = await axios.post(
      `${API_URL.replace('/auth', '/onboarding')}`,
      {
        interestedAssets,
        investorType,
        contentTypes
      },
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  },

  getPreferences: async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(
      `${API_URL.replace('/auth', '/onboarding')}`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  },

  getMarketNews: async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(
      `${API_BASE_URL}/api/dashboard/news`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  },

  getCoinPrices: async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(
      `${API_BASE_URL}/api/dashboard/prices`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  },

  getAIInsight: async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(
      `${API_BASE_URL}/api/dashboard/ai-insight`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  },

  getMeme: async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(
      `${API_BASE_URL}/api/dashboard/meme`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  },

  submitVote: async (sectionType, isPositive) => {
    const token = localStorage.getItem('token');
    const response = await axios.post(
      `${API_BASE_URL}/api/vote`,
      {
        sectionType,
        isPositive
      },
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  },
  getUserVotes: async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(
      `${API_BASE_URL}/api/vote`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    return response.data;
  }
};